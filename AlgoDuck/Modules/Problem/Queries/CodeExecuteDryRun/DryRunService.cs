using System.Text;
using System.Text.Json;
using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Modules.Problem.Shared.Types;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;
using AlgoDuck.Shared.Http;
using AlgoDuckShared;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;


public interface IExecutorDryRunService
{
    internal Task<Result<ExecutionEnqueueingResultDto, ErrorObject<string>>> DryRunUserCodeAsync(DryRunExecuteRequest request, CancellationToken cancellationToken = default);
}

internal sealed class DryRunService(
    IRabbitMqConnectionService rabbitMqConnectionService,
    IOptions<MessageQueuesConfig> messageQueuesConfig,
    IDatabase redis
    ) : IExecutorDryRunService, IAsyncDisposable
{
    private IChannel? _channel;

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken = default)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }
        var connection = await rabbitMqConnectionService.GetConnection(cancellationToken);
        _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        
        await _channel.QueueDeclareAsync(
            queue: messageQueuesConfig.Value.Execution.Write,
            durable: true,
            exclusive: false,
            autoDelete: false, cancellationToken: cancellationToken);
        
        return _channel;
    }

    public async Task<Result<ExecutionEnqueueingResultDto, ErrorObject<string>>> DryRunUserCodeAsync(DryRunExecuteRequest request, CancellationToken cancellationToken = default)
    {
        var userSolutionData = new UserSolutionData
        {
            FileContents = new StringBuilder(Encoding.UTF8.GetString(Convert.FromBase64String(request.CodeB64)))
        };

        var jobData = new ExecutionQueueJobData
        {
            JobId = userSolutionData.ExecutionId,
            UserId = request.UserId,
            SigningKey = userSolutionData.SigningKey,
            UserCodeB64 = request.CodeB64,
            JobType = JobType.DryRun
        };
        try
        {
            var analyzer = new AnalyzerSimple(userSolutionData.FileContents);
            userSolutionData.IngestCodeAnalysisResult(analyzer.AnalyzeUserCode(ExecutionStyle.Execution));
        }
        catch (JavaSyntaxException)
        {
            return await SendExecutionRequestToQueueAsync(userSolutionData, request.UserId, cancellationToken);
        }

        await redis.StringSetAsync(
            new RedisKey(userSolutionData.ExecutionId.ToString()),
            new RedisValue(JsonSerializer.Serialize(jobData)),
            TimeSpan.FromMinutes(5));
        
        return await SendExecutionRequestToQueueAsync(userSolutionData, request.UserId, cancellationToken);
    }
    
    private async Task<Result<ExecutionEnqueueingResultDto, ErrorObject<string>>> SendExecutionRequestToQueueAsync(UserSolutionData userSolutionData, Guid userId, CancellationToken cancellationToken = default)
    {
        var channel = await GetChannelAsync(cancellationToken);
        
        var innerBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new SubmitExecuteRequestRabbit
        {
            JobId = userSolutionData.ExecutionId,
            Entrypoint = userSolutionData.MainClassName,
            JavaFiles = userSolutionData.GetFileContents(),
        }));

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: messageQueuesConfig.Value.Execution.Write, 
            mandatory: false,
            basicProperties: new BasicProperties
            {
                Persistent = true
            },
            body: innerBody, 
            cancellationToken: cancellationToken);
        
        return Result<ExecutionEnqueueingResultDto, ErrorObject<string>>.Ok(new ExecutionEnqueueingResultDto
        {
            JobId = userSolutionData.ExecutionId,
            UserId = userId,
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.DisposeAsync();
    }
}