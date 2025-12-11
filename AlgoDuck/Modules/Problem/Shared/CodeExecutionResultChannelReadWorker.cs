using System.Text;
using AlgoDuckShared;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace AlgoDuck.Modules.Problem.Shared;

public sealed class CodeExecutionResultChannelReadWorker(
    IRabbitMqConnectionService rabbitMqConnectionService,
    IHubContext<ExecutionStatusHub> hubContext,
    IDatabase redis
    ) : BackgroundService, IAsyncDisposable
{
    private IChannel? _channel;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await rabbitMqConnectionService.GetConnection();
        
        _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        
        await _channel.QueueDeclareAsync(queue: "code_execution_result",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body.ToArray());
            var response = DefaultJsonSerializer.Deserialize<ExecutionResponseRabbit>(message);
            if (response != null)
            {
                SubmitExecuteResponse results;
                var signingKeyRaw = await redis.StringGetAsync(new RedisKey(response.JobId.ToString()));
                Console.WriteLine($"signing key: {Guid.Parse(signingKeyRaw.ToString())}");
                try
                {
                    var executionFileOperationHelper = new ExecutorFileOperationHelper
                    {
                        UserSolutionData = new UserSolutionData
                        {
                            SigningKey = Guid.Parse(signingKeyRaw.ToString()),
                            ExecutionId = response.JobId,
                        }
                    };
                    
                    results = executionFileOperationHelper.ParseVmOutput(response);
                }
                catch (Exception e)
                {
                    results = new SubmitExecuteResponse();
                }

                await hubContext.Clients.Group(response.JobId.ToString())
                    .SendAsync(
                        method: "ExecutionStatusUpdated",
                        arg1: results, 
                        cancellationToken: stoppingToken
                        );
            }
            await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
        };
            
        await _channel.BasicConsumeAsync(
            queue: "code_execution_results",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.DisposeAsync();
    }
}