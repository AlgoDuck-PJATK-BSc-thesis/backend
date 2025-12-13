using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RabbitMQ.Client;

namespace AlgoDuckShared;
    
public class SubmitExecuteRequestRabbit
{
    public Guid JobId { get; set; }
    public required Guid ProblemId { get; set; }
    public required Dictionary<string, string> JavaFiles { get; set; }
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubmitExecuteRequestRabbitStatus
{
    Queued,
    Compiling,
    Executing,
    Completed,
    Failed,
    TimedOut 
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
    public Task<IConnection> GetConnection();
}

public sealed class RabbitMqConnectionService(IConnectionFactory factory) : IRabbitMqConnectionService, IAsyncDisposable
{
    private IConnection? _connection;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    public async Task<IConnection> GetConnection()
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }
        await _connectionLock.WaitAsync();
        try
        {
            if (_connection is not { IsOpen: true })
            {
                _connection = await factory.CreateConnectionAsync();
            }
            return _connection;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null) await _connection.DisposeAsync();
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
