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

public sealed class ValidatorBackgroundWorker(
    ICompilationHandler compilationHandler,
    VmLaunchManager launchManager,
    IRabbitMqConnectionService rabbitMqConnectionService,
    ILogger<RabbitMqBackgroundWorker> logger,
    ServiceData serviceData
) : RabbitMqBackgroundWorker(rabbitMqConnectionService, serviceData, logger)
{
    private readonly ServiceData _serviceData = serviceData;
    private readonly ILogger<RabbitMqBackgroundWorker> _logger = logger;

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
                _logger.LogWarning("{ServiceName} Received null request, skipping", _serviceData.ServiceName);
                return;
            }

            _logger.LogInformation("{ServiceName} Processing job {JobId}", _serviceData.ServiceName, request.JobId);

            var result = await ProcessExecutionRequestAsync(request);
            await PublishResultAsync(request.JobId, result);

            _logger.LogInformation("{ServiceName}: Job {JobId} completed successfully", _serviceData.ServiceName,
                request.JobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{ServiceName}: Job {JobId} failed", _serviceData.ServiceName, request?.JobId);

            var status = ex switch
            {
                VmQueryTimedOutException => SubmitExecuteRequestRabbitStatus.TimedOut,
                _ => SubmitExecuteRequestRabbitStatus.Failed
            };
            
            var errorResult = new VmExecutionResponse
            {
                Err = GetUserFriendlyError(ex)
            };

            if (request != null)
            {
                await PublishResultAsync(request.JobId, errorResult, status);
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

    private async Task PublishResultAsync(Guid jobId, VmExecutionResponse result, SubmitExecuteRequestRabbitStatus status = SubmitExecuteRequestRabbitStatus.Completed)
    {
        var resultMessage = new ExecutionResponseRabbit
        {
            Out = result.Out,
            Err = result.Err,
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

        Console.WriteLine($"publishing to the results queue: {_serviceData.ResponseQueueName}");
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