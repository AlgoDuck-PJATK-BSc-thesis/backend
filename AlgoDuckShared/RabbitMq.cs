using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;

namespace AlgoDuckShared;
    
public class SubmitExecuteRequestRabbit
{
    public Guid JobId { get; set; }
    public required Dictionary<string, string> JavaFiles { get; set; }
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubmitExecuteRequestRabbitStatus
{
    Queued,
    Compiling,
    Executing,
    
    Completed,
    CompilationFailure,
    RuntimeError,
    ServiceFailure,
    Timeout 
}


public static class SubmitExecuteRequestRabbitStatusExtensions
{
    public static bool IsTerminalStatus(this SubmitExecuteRequestRabbitStatus status)
    {
        return status switch
        {
            SubmitExecuteRequestRabbitStatus.Completed or SubmitExecuteRequestRabbitStatus.CompilationFailure
                or SubmitExecuteRequestRabbitStatus.RuntimeError or SubmitExecuteRequestRabbitStatus.ServiceFailure
                or SubmitExecuteRequestRabbitStatus.Timeout => true,
            _ => false
        };
    }
    
    public static bool IsIntermediateStatus(this SubmitExecuteRequestRabbitStatus status)
    {
        return status switch
        {
            SubmitExecuteRequestRabbitStatus.Queued or SubmitExecuteRequestRabbitStatus.Compiling
                or SubmitExecuteRequestRabbitStatus.Executing => true,
            _ => false
        };
    }
}

public class ExecutionResponseRabbit
{
    public required Guid JobId { get; set; }
    public required SubmitExecuteRequestRabbitStatus Status { get; set; }
    public string Out { get; set; } = string.Empty;
    public string Err { get; set; } = string.Empty;
    public int ExitCode { get; set; }
    public long StartNs { get; set; }
    public long EndNs { get; set; }
    public long MaxMemoryKb { get; set; }
}

public interface IRabbitMqConnectionService
{
    Task<IConnection> GetConnection(CancellationToken cancellationToken = default);
}

public sealed class RabbitMqConnectionService(
    IConnectionFactory factory,
    ILogger<RabbitMqConnectionService> logger) : IRabbitMqConnectionService, IAsyncDisposable
{
    private IConnection? _connection;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private static readonly ResiliencePipeline<IConnection> RetryPipeline = new ResiliencePipelineBuilder<IConnection>()
        .AddRetry(new RetryStrategyOptions<IConnection>
        {
            MaxRetryAttempts = 10,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromSeconds(2),
            MaxDelay = TimeSpan.FromSeconds(60),
            UseJitter = true
        })
        .Build();

    public async Task<IConnection> GetConnection(CancellationToken cancellationToken = default)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            _connection = await RetryPipeline.ExecuteAsync(async ct =>
            {
                logger.LogInformation("Attempting to connect to RabbitMQ...");
                return await factory.CreateConnectionAsync(ct);
            }, cancellationToken);

            logger.LogInformation("Successfully connected to RabbitMQ");
            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }
}

internal class ChannelWriteOpts<T> where T : class
{
    internal required string ChannelName { get; set; }
    internal required IChannel Channel { get; init; }
    internal required T Message { get; init; }
}

internal class ChannelSetupOpts
{
    internal required IChannel Channel { get; init; }
    internal List<string> ChannelNames { get; set; } = [];
}

public record QueueDeclareOptions(bool Durable, bool Exclusive, bool AutoDelete);

internal static class ChannelExtensions
{
    public static Task QueueDeclareAsync(this IChannel channel, string queue, QueueDeclareOptions options)
    {
        return channel.QueueDeclareAsync(
            queue: queue,
            durable: options.Durable,
            exclusive: options.Exclusive,
            autoDelete: options.AutoDelete);
    }
}

public class ChannelDeclareDto
{
    public required string ChannelName { get; set; }
    public required QueueDeclareOptions QueueDeclareOptions { get; set; }
}

public interface IRabbitMqChannelFactory
{
    public Task<IChannel> GetChannelAsync(params ChannelDeclareDto[] declareDtoList);
}

public class RabbitMqChannelFactory(IRabbitMqConnectionService connectionService) : IRabbitMqChannelFactory
{
    private IChannel? _channel;

    private static class QueueNames
    {
        public const string ValidationRequests = "problem_validation_requests";
        public const string ValidationResults = "problem_validation_results";
    }

    public async Task<IChannel> GetChannelAsync(params ChannelDeclareDto[] declareDtoList)
    {
        if (_channel is { IsOpen: true })
            return _channel;

        var connection = await connectionService.GetConnection();
        _channel = await connection.CreateChannelAsync();

        foreach (var channelDeclareDto in declareDtoList)
        {
            await DeclareQueuesAsync(_channel, channelDeclareDto);
            
        }
        return _channel;
    }

    private static async Task DeclareQueuesAsync(IChannel channel, ChannelDeclareDto channelDeclareDto)
    {
        // var queueOptions = new QueueDeclareOptions(Durable: true, Exclusive: false, AutoDelete: false);

        var queueOptions = new QueueDeclareOptions(
            Durable: channelDeclareDto.QueueDeclareOptions.Durable,
            Exclusive: channelDeclareDto.QueueDeclareOptions.Exclusive,
            AutoDelete: channelDeclareDto.QueueDeclareOptions.AutoDelete);

        await channel.QueueDeclareAsync(channelDeclareDto.ChannelName, queueOptions);

    }
}
