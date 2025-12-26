using AlgoDuckShared;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ExecutorService.Executor.BackgroundWorkers;

public abstract class RabbitMqBackgroundWorker(
    IRabbitMqConnectionService rabbitMqConnectionService,
    ServiceData serviceData,
    ILogger<RabbitMqBackgroundWorker> logger) : BackgroundService, IAsyncDisposable
{
    protected IChannel? Channel;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{ServiceName} starting", serviceData.ServiceName);
        
        try
        {
            var connection = await rabbitMqConnectionService.GetConnection();
            Channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await SetupQueues(stoppingToken);
            await SetupQos(GetQosOptions(), stoppingToken);
            
            await StartConsumerAsync(stoppingToken);
            
            logger.LogInformation("{ServiceName} started, waiting for messages", serviceData.ServiceName);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("{ServiceName} stopping due to cancellation", serviceData.ServiceName);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "{ServiceName} failed to start", serviceData.ServiceName);
            throw;
        }
    }
    
    private async Task SetupQueues(CancellationToken cancellationToken = default)
    {
        if (Channel == null) throw new InvalidOperationException("Channel not initialized");
        
        await Channel.QueueDeclareAsync(
            queue: serviceData.RequestQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await Channel.QueueDeclareAsync(
            queue: serviceData.ResponseQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }
    
    private async Task SetupQos(BasicQosOptions options, CancellationToken cancellationToken = default)
    {
        if (Channel == null) throw new InvalidOperationException("Channel not initialized");
        await Channel.BasicQosAsync(
            prefetchSize: options.PrefetchSize, 
            prefetchCount: options.PrefetchCount, 
            global: options.Global,
            cancellationToken: cancellationToken);
    }
    
    private async Task StartConsumerAsync(CancellationToken ct = default)
    {
        if (Channel == null) throw new InvalidOperationException("Channel not initialized");
        
        var consumer = new AsyncEventingBasicConsumer(Channel);
        consumer.ReceivedAsync += async (sender, ea) => await HandleMessageAsync(sender, ea, ct);

        await Channel.BasicConsumeAsync(
            queue: serviceData.RequestQueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct);
    }
    
    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs ea, CancellationToken cancellationToken = default)
    {
        try
        {
            await ProcessMessageAsync(ea, cancellationToken);
            
            if (Channel != null)
            {
                await Channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message");
            
            if (Channel != null)
            {
                await Channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
            }
        }
    }
    
    protected abstract Task ProcessMessageAsync(BasicDeliverEventArgs ea, CancellationToken cancellationToken = default);
    
    protected virtual BasicQosOptions GetQosOptions() => new();
    
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        logger.LogInformation("{ServiceName} disposing", serviceData.ServiceName);
        
        if (Channel != null)
        {
            await Channel.CloseAsync();
            await Channel.DisposeAsync();
        }
    }
}

public class ServiceData
{
    public required string ServiceName { get; set; }
    public required string RequestQueueName { get; set; }
    public required string ResponseQueueName { get; set; }
}

public class BasicQosOptions
{
    public uint PrefetchSize { get; set; } = 0;
    public ushort PrefetchCount { get; set; } = 10;
    public bool Global { get; set; } = false;
}