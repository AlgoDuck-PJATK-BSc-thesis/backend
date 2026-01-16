using System.Text;
using System.Text.Json;
using AlgoDuckShared;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.Types;
using ExecutorService.Executor.Types.Config;
using ExecutorService.Executor.Types.FilesystemPoolerTypes;
using ExecutorService.Executor.Types.VmLaunchTypes;
using ExecutorService.Executor.VmLaunchSystem;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ExecutorService.Executor.BackgroundWorkers;

public sealed class ExecutorBackgroundWorker(
    ICompilationHandler compilationHandler,
    VmLaunchManager launchManager,
    IRabbitMqConnectionService rabbitMqConnectionService,
    ILogger<ExecutorBackgroundWorker> logger,
    ServiceData serviceData
    ) : RabbitMqBackgroundWorker(rabbitMqConnectionService, serviceData, logger)
{
    
    private readonly ServiceData _serviceData = serviceData;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
    protected override async Task ProcessMessageAsync(BasicDeliverEventArgs ea, CancellationToken cancellationToken = default)
    {
        SubmitExecuteRequestRabbit? request = null;
    
        try
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            request = JsonSerializer.Deserialize<SubmitExecuteRequestRabbit>(message);

            if (request == null)
            {
                logger.LogWarning("{ServiceName} Received null request, skipping", _serviceData.ServiceName);
                return; 
            }

            logger.LogInformation("{ServiceName} Processing job {JobId}", _serviceData.ServiceName, request.JobId);
        
            var result = await ProcessExecutionRequestAsync(request);
            var status = result.Err.Equals(string.Empty) ? SubmitExecuteRequestRabbitStatus.Completed : SubmitExecuteRequestRabbitStatus.RuntimeError;

            await PublishResultAsync(request.JobId, result, status);

            logger.LogInformation("{ServiceName}: Job {JobId} completed successfully", _serviceData.ServiceName, request.JobId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ServiceName}: Job {JobId} failed", _serviceData.ServiceName, request?.JobId);
        
            
            var status = ex switch
            {
                VmQueryTimedOutException => SubmitExecuteRequestRabbitStatus.Timeout,
                CompilationException => SubmitExecuteRequestRabbitStatus.CompilationFailure,
                _ => SubmitExecuteRequestRabbitStatus.ServiceFailure
            };
            
            var errorResult = new VmExecutionResponse
            {
                Err = GetUserFriendlyError(ex),
            };
        
            if (request != null)
            {
                await PublishResultAsync(request.JobId, errorResult, status);
            }
        }
    }
    
    
    private Result<ExecutionResponseRabbit, ErrorObject<string>> ReadRequestFromChannel(BasicDeliverEventArgs deliverEventArgs)
    {
        var body = deliverEventArgs.Body;
        try
        {
            var message = Encoding.UTF8.GetString(body.ToArray());
            
            var response = JsonSerializer.Deserialize<ExecutionResponseRabbit>(message, JsonOptions);
            if (response == null)
            {
                logger.LogWarning("{ReadWorkerName}: Read null message from channel {channelName}", GetType().Name, _serviceData.ServiceName);
                return Result<ExecutionResponseRabbit, ErrorObject<string>>.Err(ErrorObject<string>.NotFound("Null value"));
            }
            return Result<ExecutionResponseRabbit, ErrorObject<string>>.Ok(response);
        }
        catch (JsonException e)
        {
            logger.LogError(e, "{ReadWorkerName} Error reading job data. Invalid format", GetType().Name);
            return Result<ExecutionResponseRabbit, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest("Bad format"));
        }
        catch (Exception e)
        {
            logger.LogError(e, "{ReadWorkerName} Error reading job data.", GetType().Name);
            return Result<ExecutionResponseRabbit, ErrorObject<string>>.Err(ErrorObject<string>.InternalError("Error reading job data"));
        }
    }
    
    private async Task<VmExecutionResponse> ProcessExecutionRequestAsync(SubmitExecuteRequestRabbit request)
    {

        var vmLeaseTask = launchManager.AcquireVmAsync(FilesystemType.Executor);
        
        VmLease? vmLease = null;
        try
        {
            await PublishStatusAsync(request.JobId, SubmitExecuteRequestRabbitStatus.Compiling);
            var compilationResult = await compilationHandler.CompileAsync(new VmJobRequestInterface<VmCompilationPayload>
            {
                JobId = request.JobId,
                Payload = new VmCompilationPayload
                {
                    JobId = request.JobId,
                    SrcFiles = request.JavaFiles
                }
            });
            if (compilationResult is VmCompilationFailure failure)
            {
                throw new CompilationException(failure.Body);
            }

            vmLease = await vmLeaseTask;
            
            await PublishStatusAsync(request.JobId, SubmitExecuteRequestRabbitStatus.Executing);

            var successResult = (VmCompilationSuccess) compilationResult;
            return await vmLease.QueryAsync<VmExecutionPayload, VmExecutionResponse>(new VmJobRequestInterface<VmExecutionPayload>()
            {
                JobId = request.JobId,
                Payload = new VmExecutionPayload
                {
                    ClientSrc = successResult.Body,
                    Entrypoint = request.Entrypoint
                }
            });
        }
        finally
        {
            if (vmLease != null)
            {
                vmLease.Dispose();
            }
            else if (vmLeaseTask.IsCompletedSuccessfully)
            {
                vmLeaseTask.Result.Dispose();
            }
            else if (!vmLeaseTask.IsCompleted)
            {
                _ = vmLeaseTask.ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        t.Result.Dispose();
                    }
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }
    }
    
    private async Task PublishStatusAsync(Guid jobId, SubmitExecuteRequestRabbitStatus status)
    {
        if (Channel == null) return;
        
        var statusMessage = new ExecutionResponseRabbit
        {
            JobId = jobId,
            Status = status,
        };

        await PublishToResultsQueueAsync(statusMessage);
    }

    private async Task PublishResultAsync(Guid jobId, VmExecutionResponse result, SubmitExecuteRequestRabbitStatus status = SubmitExecuteRequestRabbitStatus.Completed)
    {
        var resultMessage = new ExecutionResponseRabbit
        {
            Out = result.Out,
            Err = result.Err,
            ExitCode = CoalescedIntParse(result.ExitCode),
            StartNs = CoalescedLongParse(result.StartNs),
            EndNs = CoalescedLongParse(result.EndNs),
            MaxMemoryKb = CoalescedLongParse(result.MaxMemoryKb),
            JobId = jobId,
            Status = status
        };

        await PublishToResultsQueueAsync(resultMessage);
    }

    private async Task PublishToResultsQueueAsync<T>(T message)
    {
        if (Channel == null) return;
        
        var json = JsonSerializer.Serialize(message, JsonOptions);
        var body = Encoding.UTF8.GetBytes(json);

        await Channel.BasicPublishAsync(
            exchange: "",
            routingKey: _serviceData.ResponseQueueName,
            mandatory: false,
            basicProperties: new BasicProperties { Persistent = true },
            body: body);
    }

    private static long CoalescedLongParse(string value)
    {
        try
        {
            return long.Parse(value);
        }
        catch (Exception e)
        {
            return 0L;
        }
    }
    
    private static int CoalescedIntParse(string value)
    {
        try
        {
            return int.Parse(value);
        }
        catch (Exception e)
        {
            return 0;
        }
    }
    
    private static string GetUserFriendlyError(Exception ex)
    {
        return ex switch
        {
            CompilationException ce => ce.Message,
            VmQueryTimedOutException => "Execution timed out. Your code may have an infinite loop.",
            VmClusterOverloadedException => "Service is busy. Please try again in a moment.",
            _ => "An unexpected error occurred during execution."
        };
    }

    private async Task AcknowledgeAsync(ulong deliveryTag)
    {
        if (Channel == null) return;
        await Channel.BasicAckAsync(deliveryTag, multiple: false);
    }
    

    protected override BasicQosOptions GetQosOptions() => new()
    {
        PrefetchCount = 5
    };
}