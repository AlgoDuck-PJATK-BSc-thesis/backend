using System.Text;
using System.Text.Json;
using AlgoDuck.DAL;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;
using AlgoDuck.Shared.Http;
using AlgoDuckShared;
using OneOf.Types;
using RabbitMQ.Client;

namespace AlgoDuck.Modules.Problem.Commands.CreateProblem;

public interface ICreateProblemService
{
    public Task<Result<CreateProblemResultDto, ErrorObject<string>>> CreateProblemAsync(CreateProblemDto problemDto,
        CancellationToken cancellationToken = default);
}

public class CreateProblemService(
    IRabbitMqConnectionService rabbitMqConnectionService,
    ICreateProblemRepository createProblemRepository
) : ICreateProblemService
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
    
    public async Task<Result<CreateProblemResultDto, ErrorObject<string>>> CreateProblemAsync(
        CreateProblemDto problemDto, CancellationToken cancellationToken = default)
    {
        var channel = await GetChannelAsync();
        
        var solutionData = new UserSolutionData
        {
            FileContents = new StringBuilder(Encoding.UTF8.GetString(Convert.FromBase64String(problemDto.TemplateB64)))
        };
        
        var analyzer = new AnalyzerSimple(solutionData.FileContents);
        
        solutionData.IngestCodeAnalysisResult(analyzer.AnalyzeUserCode(ExecutionStyle.Execution));
        
        var helper = new ExecutorFileOperationHelper
        {
            UserSolutionData = solutionData
        };
        
        var testCases = problemDto.TestCases.Select(t => new TestCaseJoined
        {
            TestCaseId = Guid.NewGuid(),
            Call = t.CallArgs.Select(ca => ca.Name).ToArray(),
            CallFunc = t.CallMethod.MethodName, /*TODO: This won't work. We have to actually resolve where the method is from*/
            Display = t.Display,
            DisplayRes = t.DisplayRes,
            Expected = t.Expected.Name,
            IsPublic = t.IsPublic,
            ProblemProblemId = Guid.Empty,
            Setup = Encoding.UTF8.GetString(Convert.FromBase64String(t.ArrangeB64))
        }).ToList();
        
        helper.InsertTestCases(testCases, solutionData.MainClassName);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new SubmitExecuteRequestRabbit
        {
            JobId = solutionData.ExecutionId,
            ProblemId = Guid.Empty,
            JavaFiles = solutionData.GetFileContents(),
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
        
        return await createProblemRepository.CreateProblemAsync(problemDto, cancellationToken);
    }
}