using System.Text;
using System.Text.Json;
using AlgoDuckShared;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.ResourceHandlers;
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
    
    protected override async Task ProcessMessageAsync(BasicDeliverEventArgs ea)
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
            await PublishResultAsync(request.JobId, result);
        
            logger.LogInformation("{ServiceName}: Job {JobId} completed successfully", _serviceData.ServiceName, request.JobId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{ServiceName}: Job {JobId} failed", _serviceData.ServiceName, request?.JobId);
        
            var errorResult = new VmExecutionResponse
            {
                Err = GetUserFriendlyError(ex)
            };
        
            if (request != null)
            {
                await PublishResultAsync(request.JobId, errorResult);
            }
        }
    }
    
    private async Task<VmExecutionResponse> ProcessExecutionRequestAsync(SubmitExecuteRequestRabbit request)
    {
        await PublishStatusAsync(request.JobId, SubmitExecuteRequestRabbitStatus.Compiling);

        var vmLeaseTask = launchManager.AcquireVmAsync(FilesystemType.Executor);
        
        VmLease? vmLease = null;
        try
        {
            var compilationResult = await compilationHandler.CompileAsync(request);
            
            if (compilationResult is VmCompilationFailure failure)
            {
                throw new CompilationException(failure.ErrorMsg);
            }

            vmLease = await vmLeaseTask;
            
            await PublishStatusAsync(request.JobId, SubmitExecuteRequestRabbitStatus.Executing);

            var successResult = (VmCompilationSuccess)compilationResult;
            var result = await vmLease.QueryAsync<VmExecutionQuery, VmExecutionResponse>(
                new VmExecutionQuery(successResult));

            return result;
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

    private async Task PublishResultAsync(Guid jobId, VmExecutionResponse result)
    {
        var resultMessage = new ExecutionResponseRabbit
        {
            Out = result.Out,
            Err = result.Err,
            JobId = jobId,
            Status = SubmitExecuteRequestRabbitStatus.Completed
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