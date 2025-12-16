using System.Text;
using System.Text.Json;
using AlgoDuckShared;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.Types.VmLaunchTypes;
using ExecutorService.Executor.VmLaunchSystem;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ExecutorService.Executor;

public sealed class CodeExecutorService : BackgroundService, IAsyncDisposable
{
    private readonly ICompilationHandler _compilationHandler;
    private readonly VmLaunchManager _launchManager;
    private readonly IRabbitMqConnectionService _rabbitMqConnectionService;
    private readonly ILogger<CodeExecutorService> _logger;
    private readonly ExecutorConfiguration _config;
    
    private IChannel? _channel;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CodeExecutorService(
        ICompilationHandler compilationHandler,
        VmLaunchManager launchManager,
        IRabbitMqConnectionService rabbitMqConnectionService,
        ILogger<CodeExecutorService> logger,
        IOptions<ExecutorConfiguration> config)
    {
        _compilationHandler = compilationHandler;
        _launchManager = launchManager;
        _rabbitMqConnectionService = rabbitMqConnectionService;
        _logger = logger;
        _config = config.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CodeExecutorService starting");
        
        try
        {
            var connection = await _rabbitMqConnectionService.GetConnection();
            _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await SetupQueuesAsync(stoppingToken);
            await StartConsumerAsync(stoppingToken);
            
            _logger.LogInformation("CodeExecutorService started, waiting for messages");
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("CodeExecutorService stopping due to cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "CodeExecutorService failed to start");
            throw;
        }
    }

    private async Task SetupQueuesAsync(CancellationToken ct)
    {
        if (_channel == null) throw new InvalidOperationException("Channel not initialized");
        
        await _channel.QueueDeclareAsync(
            queue: "code_execution_requests",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct);

        await _channel.QueueDeclareAsync(
            queue: "code_execution_results",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct);

        await _channel.BasicQosAsync(
            prefetchSize: 0, 
            prefetchCount: 10, 
            global: false,
            cancellationToken: ct);
    }

    private async Task StartConsumerAsync(CancellationToken ct)
    {
        if (_channel == null) throw new InvalidOperationException("Channel not initialized");
        
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: "code_execution_requests",
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        var deliveryTag = ea.DeliveryTag;
        SubmitExecuteRequestRabbit? request = null;
        
        try
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            request = JsonSerializer.Deserialize<SubmitExecuteRequestRabbit>(message);

            if (request == null)
            {
                _logger.LogWarning("Received null request, acknowledging and skipping");
                await AcknowledgeAsync(deliveryTag);
                return;
            }

            _logger.LogInformation("Processing job {JobId}", request.JobId);
            
            var result = await ProcessExecutionRequestAsync(request);
            await PublishResultAsync(request.JobId, result);
            await AcknowledgeAsync(deliveryTag);
            
            _logger.LogInformation("Job {JobId} completed successfully", request.JobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed", request?.JobId);
            
            var errorResult = new VmExecutionResponse
            {
                Err = GetUserFriendlyError(ex)
            };
            
            if (request != null)
            {
                await PublishResultAsync(request.JobId, errorResult);
            }
            
            await AcknowledgeAsync(deliveryTag);
        }
    }

    private async Task<VmExecutionResponse> ProcessExecutionRequestAsync(SubmitExecuteRequestRabbit request)
    {
        await PublishStatusAsync(request.JobId, SubmitExecuteRequestRabbitStatus.Compiling);

        var vmLeaseTask = _launchManager.AcquireVmAsync(FilesystemType.Executor);
        
        VmLease? vmLease = null;
        try
        {
            var compilationResult = await _compilationHandler.CompileAsync(request);
            
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
        if (_channel == null) return;
        
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
        if (_channel == null) return;
        
        var json = JsonSerializer.Serialize(message, JsonOptions);
        var body = Encoding.UTF8.GetBytes(json);

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: "code_execution_results",
            mandatory: false,
            basicProperties: new BasicProperties { Persistent = true },
            body: body);
    }

    private async Task AcknowledgeAsync(ulong deliveryTag)
    {
        if (_channel == null) return;
        await _channel.BasicAckAsync(deliveryTag, multiple: false);
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

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("CodeExecutorService disposing");
        
        if (_channel != null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }
    }
}