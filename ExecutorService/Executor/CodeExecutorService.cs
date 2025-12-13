using System.Text;
using System.Text.Json;
using AlgoDuckShared;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.Types.VmLaunchTypes;
using ExecutorService.Executor.VmLaunchSystem;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ExecutorService.Executor;

internal class ChannelWriteOpts<T> where T : class
{
    internal string ChannelName { get; set; } = "code_execution_results";
    internal required IChannel Channel { get; init; }
    internal required T Message { get; init; }
}

internal class ChannelSetupOpts
{
    internal required IChannel Channel { get; init; }
    internal List<string> ChannelNames { get; set; } = [];
}

public sealed class CodeExecutorService(
    ICompilationHandler compilationHandler,
    VmLaunchManager launchManager,
    IRabbitMqConnectionService rabbitMqConnectionService)
    : BackgroundService, IAsyncDisposable
{
    private IChannel? _channel;
    
    private static async Task WriteToChannelDefault<T>(ChannelWriteOpts<T> writeOpts, CancellationToken cancellationToken = default) where T : class
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

    private static async Task SetupChannels(ChannelSetupOpts channelSetupOpts, CancellationToken cancellationToken = default)
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
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await rabbitMqConnectionService.GetConnection();
        _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        
        await _channel.QueueDeclareAsync(
            queue: "code_execution_requests",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: "code_execution_results",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false,
            cancellationToken: stoppingToken);
        
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body.ToArray());

            var request = JsonSerializer.Deserialize<SubmitExecuteRequestRabbit>(message);
            Console.WriteLine(request?.JobId);
            VmExecutionResponse result;
            
            try
            {
                if (request != null)
                {
                    Console.WriteLine(JsonSerializer.Serialize(new ExecutionResponseRabbit
                    {
                        JobId = request.JobId,
                        Status = SubmitExecuteRequestRabbitStatus.Compiling,
                    }));
                    await _channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: "code_execution_results",
                        mandatory: false,
                        basicProperties: new BasicProperties { Persistent = true },
                        body: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ExecutionResponseRabbit
                        {
                            JobId = request.JobId,
                            Status = SubmitExecuteRequestRabbitStatus.Compiling,
                        })), 
                        cancellationToken: stoppingToken);

                    var vmLeaseTask = launchManager.AcquireVmAsync(FilesystemType.Executor); 
                    var compilationResult = await compilationHandler.CompileAsync(request);
                    using var vmLease = await vmLeaseTask;
                    if (compilationResult is VmCompilationFailure failure)
                    {
                        throw new CompilationException(failure.ErrorMsg);
                    }
            
                    await _channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: "code_execution_results",
                        mandatory: false,
                        basicProperties: new BasicProperties { Persistent = true },
                        body: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ExecutionResponseRabbit
                        {
                            JobId = request.JobId,
                            Status = SubmitExecuteRequestRabbitStatus.Executing,
                        })), 
                        cancellationToken: stoppingToken);

                    result = await vmLease.QueryAsync<VmExecutionQuery, VmExecutionResponse>(new VmExecutionQuery((compilationResult as VmCompilationSuccess)!));
                }
                else
                {
                    result = new VmExecutionResponse();
                }
            }
            catch (CompilationException e)
            {
                result = new VmExecutionResponse
                {
                    Err = e.Message
                };
            }
            catch (VmQueryTimedOutException)
            {
                result = new VmExecutionResponse
                {
                    Err = "Query timed out"
                };
            }
            catch (Exception)
            {
                result = new VmExecutionResponse
                {
                    Err = "Internal server error",
                };
            }

            var resultJson = JsonSerializer.Serialize(new ExecutionResponseRabbit
            {
                Out = result.Out,
                Err = result.Err,
                JobId = request?.JobId ?? Guid.Empty,
                Status = SubmitExecuteRequestRabbitStatus.Completed
            });
            
            var resultBody = Encoding.UTF8.GetBytes(resultJson);

            var props = new BasicProperties { Persistent = true };

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: "code_execution_results",
                mandatory: false,
                basicProperties: props,
                body: resultBody, 
                cancellationToken: stoppingToken);

            await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
        };
        
        await _channel.BasicConsumeAsync(
            queue: "code_execution_requests",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }
    }
}
