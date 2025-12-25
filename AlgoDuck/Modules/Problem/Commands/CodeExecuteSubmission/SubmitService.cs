using System.Text;
using System.Text.Json;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;
using AlgoDuckShared;
using RabbitMQ.Client;
using StackExchange.Redis;


namespace AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;



public interface IExecutorSubmitService
{

    public Task<Guid> SubmitUserCodeRabbit(SubmitExecuteRequest submission, Guid userId,
        CancellationToken cancellationToken = default);

}

internal sealed class SubmitService(
    IRabbitMqConnectionService rabbitMqConnectionService,
    IExecutorSubmitRepository executorSubmitRepository,
    IDatabase redis
    ) : IExecutorSubmitService, IAsyncDisposable
{
    private IChannel? _channel;

    private async Task<IChannel> GetChannelAsync()
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }
        var connection = await rabbitMqConnectionService.GetConnection();
        _channel = await connection.CreateChannelAsync();
        
        await _channel.QueueDeclareAsync(
            queue: "code_execution_requests",
            durable: true,
            exclusive: false,
            autoDelete: false);
        
        await _channel.QueueDeclareAsync(
            queue: "code_execution_results",
            durable: true,
            exclusive: false,
            autoDelete: false);
        
        return _channel;
    }
    
    public async Task<Guid> SubmitUserCodeRabbit(SubmitExecuteRequest submission, Guid userId, CancellationToken cancellationToken = default)
    {   
        var channel = await GetChannelAsync();
        
        var userSolutionData = new UserSolutionData
        {
            FileContents = new StringBuilder(Encoding.UTF8.GetString(Convert.FromBase64String(submission.CodeB64)))
        };
        
        var exerciseTemplate = await executorSubmitRepository.GetTemplateAsync(submission.ProblemId);
        
        var analyzer = new AnalyzerSimple(userSolutionData.FileContents, exerciseTemplate.Template);
        userSolutionData.IngestCodeAnalysisResult(analyzer.AnalyzeUserCode(ExecutionStyle.Submission));
        
        await redis.StringSetAsync(
            new RedisKey(userSolutionData.ExecutionId.ToString()),
            new RedisValue(userSolutionData.SigningKey.ToString()),
            TimeSpan.FromMinutes(5));
        
        var helper = new ExecutorFileOperationHelper
        {
            UserSolutionData = userSolutionData
        };
        
        
        helper.InsertTestCases(await executorSubmitRepository.GetTestCasesAsync(submission.ProblemId), userSolutionData.MainClassName);
        helper.InsertTiming();
        helper.InsertGsonImport();
        
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new SubmitExecuteRequestRabbit
        {
            JobId = userSolutionData.ExecutionId,
            ProblemId = submission.ProblemId,
            JavaFiles = userSolutionData.GetFileContents(),
        }));

        var props = new BasicProperties
        {
            Persistent = true
        };
        
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "code_execution_requests",
            mandatory: false,
            basicProperties: props,
            body: body, 
            cancellationToken: cancellationToken);
        
        return userSolutionData.ExecutionId;
        
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.DisposeAsync();
    }
}



