using System.Text;
using System.Text.Json;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;
using AlgoDuck.Shared.Http;
using AlgoDuckShared;
using Microsoft.Extensions.Options;
using OneOf.Types;
using RabbitMQ.Client;
using StackExchange.Redis;


namespace AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;



public interface IExecutorSubmitService
{

    public Task<Result<ExecutionEnqueueingResultDto, ErrorObject<string>>> SubmitUserCodeRabbitAsync(SubmitExecuteRequest submission, CancellationToken cancellationToken = default);

}

internal sealed class SubmitService(
    IRabbitMqConnectionService rabbitMqConnectionService,
    IExecutorSubmitRepository executorSubmitRepository,
    IOptions<MessageQueuesConfig> messageQueuesConfig,
    IDatabase redis
    ) : IExecutorSubmitService, IAsyncDisposable
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
    
    public async Task<Result<ExecutionEnqueueingResultDto, ErrorObject<string>>> SubmitUserCodeRabbitAsync(SubmitExecuteRequest submission, CancellationToken cancellationToken = default)
    {   
        
        var userSolutionData = new UserSolutionData
        {
            FileContents = new StringBuilder(Encoding.UTF8.GetString(Convert.FromBase64String(submission.CodeB64)))
        };
        
        
        var jobData = new ExecutionQueueJobData
        {
            JobId = userSolutionData.ExecutionId,
            UserId = submission.UserId,
            SigningKey = userSolutionData.SigningKey,
            UserCodeB64 = submission.CodeB64,
            ProblemId = submission.ProblemId,
            JobType = JobType.Testing
        };
        
        await redis.StringSetAsync(
            new RedisKey(jobData.JobId.ToString()),
            new RedisValue(JsonSerializer.Serialize(jobData)),
            TimeSpan.FromMinutes(5));
        
        var exerciseTemplate = await executorSubmitRepository.GetTemplateAsync(submission.ProblemId);
        try
        {
            var analyzer = new AnalyzerSimple(userSolutionData.FileContents, exerciseTemplate.Template);
            userSolutionData.IngestCodeAnalysisResult(analyzer.AnalyzeUserCode(ExecutionStyle.Submission));
        }
        catch (JavaSyntaxException)
        {
            return await SendExecutionRequestToQueueAsync(userSolutionData, submission.UserId,  cancellationToken);
        }
        
        var helper = new ExecutorFileOperationHelper
        {
            UserSolutionData = userSolutionData
        };

        var testCases = await executorSubmitRepository.GetTestCasesAsync(submission.ProblemId);
        
        helper.InsertTestCases(testCases, userSolutionData.MainClassName);
        helper.InsertTiming();
        helper.InsertGsonImport();

        return await SendExecutionRequestToQueueAsync(userSolutionData, submission.UserId,  cancellationToken);
    }

    private async Task<Result<ExecutionEnqueueingResultDto, ErrorObject<string>>> SendExecutionRequestToQueueAsync(UserSolutionData userSolutionData, Guid userId, CancellationToken cancellationToken = default)
    {
        var channel = await GetChannelAsync();
        
        var innerBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new SubmitExecuteRequestRabbit
        {
            JobId = userSolutionData.ExecutionId,
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


