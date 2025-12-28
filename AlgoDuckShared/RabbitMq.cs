using System.Text;
using System.Text.Json;
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

internal static class RabbitMqUtils
{
    internal static async Task WriteToChannelDefaultAsync<T>(ChannelWriteOpts<T> writeOpts, CancellationToken cancellationToken = default) where T : class
    {
        var message = JsonSerializer.Serialize(writeOpts);
        var body = Encoding.UTF8.GetBytes(message);
        await writeOpts.Channel.BasicPublishAsync(
            exchange: "",
            routingKey: writeOpts.ChannelName,
            mandatory: false,
            basicProperties: new BasicProperties
            {
                Persistent = true
            },
            body: body,
            cancellationToken: cancellationToken);
    }

    internal static async Task SetupChannelsAsync(ChannelSetupOpts channelSetupOpts, CancellationToken cancellationToken = default)
    {
        foreach (var channelName in channelSetupOpts.ChannelNames)
        {
            await channelSetupOpts.Channel.QueueDeclareAsync(
                queue: channelName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);
        }
    }
}