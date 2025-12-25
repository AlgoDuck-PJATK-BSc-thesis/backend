using System.Text;
using System.Text.Json;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;
using AlgoDuck.Shared.Http;
using AlgoDuckShared;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace AlgoDuck.Modules.Problem.Commands.CreateProblem;

public interface ICreateProblemService
{
    public Task<Result<CreateUnverifiedProblemDto, ErrorObject<string>>> CreateProblemAsync(CreateProblemDto problemDto,
        CancellationToken cancellationToken = default);
}

public class CreateProblemService(
    IRabbitMqConnectionService rabbitMqConnectionService,
    IDatabase redis,
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
            queue: "problem_validation_requests",
            durable: true,
            exclusive: false,
            autoDelete: false);
        
        await _channel.QueueDeclareAsync(
            queue: "problem_validation_results",
            durable: true,
            exclusive: false,
            autoDelete: false);
        return _channel;
    }
    
    public async Task<Result<CreateUnverifiedProblemDto, ErrorObject<string>>> CreateProblemAsync(
        CreateProblemDto problemDto, CancellationToken cancellationToken = default)
    {
        var channel = await GetChannelAsync();
        
        var solutionData = new UserSolutionData
        {
            FileContents = new StringBuilder(Encoding.UTF8.GetString(Convert.FromBase64String(problemDto.TemplateB64)))
        };

        var analyzer = new AnalyzerSimple(solutionData.FileContents);

        var analysisResult = analyzer.AnalyzeUserCode(ExecutionStyle.Execution);
        solutionData.IngestCodeAnalysisResult(analysisResult);
        
        var helper = new ExecutorFileOperationHelper
        {
            UserSolutionData = solutionData
        };
        
        foreach (var testCase in problemDto.TestCases)
        {
            var resolvedFuncName = analyzer.RecursiveResolveFunctionCall(analysisResult.Main.FuncScope!.OwnScope,
                testCase.CallMethod.QualifiedName.Split('.'));
            
            if (resolvedFuncName.IsErr)
                return Result<CreateUnverifiedProblemDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest($"cannot resolve function: {testCase.CallMethod.MethodName}"));
            
            testCase.ResolvedFunctionCall = resolvedFuncName.AsT0;
        }

        var arranges = problemDto.TestCases.Select(tc =>
        {
            var tcArrange = Encoding.UTF8.GetString(Convert.FromBase64String(tc.ArrangeB64));
            var tcAnalyzer = new AnalyzerSimple(new StringBuilder(tcArrange));
            var tcAnalysisResult = tcAnalyzer.AnalyzeUserCode(ExecutionStyle.Execution);
            var mainLen = tcAnalysisResult.MainMethodIndices == null
                ? 0
                : tcAnalysisResult.MainMethodIndices.MethodFileEndIndex -
                  tcAnalysisResult.MainMethodIndices.MethodFileBeginIndex - 1;
            return tcArrange.Substring(tcAnalysisResult.MainMethodIndices?.MethodFileBeginIndex + 1 ?? 0, mainLen);
        }).ToList();
        
        var testCases = problemDto.TestCases.Select((t, i) => new TestCaseJoined
        {
            TestCaseId = Guid.NewGuid(),
            Call = t.CallArgs.Select(ca => ca.Name).ToArray(),
            CallFunc = t.ResolvedFunctionCall ?? "", 
            Display = t.Display,
            DisplayRes = t.DisplayRes,
            Expected = t.Expected.Name,
            IsPublic = t.IsPublic,
            ProblemProblemId = Guid.Empty,
            Setup = arranges[i]
        }).ToList();
        
        helper.InsertGsonImport();
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
            routingKey: "problem_validation_requests",
            mandatory: false,
            basicProperties: props,
            body: body, 
            cancellationToken: cancellationToken);
        
        var addResult = await createProblemRepository.CreateProblemAsync(problemDto, cancellationToken);


        return await addResult.Match<Task<Result<CreateUnverifiedProblemDto, ErrorObject<string>>>>(
            async ok =>
            {
                var jobData = JsonSerializer.Serialize(new JobData
                {
                    CommissioningUserId = problemDto.CreatingUserId,
                    ProblemId = ok
                });
                await redis.StringSetAsync(new RedisKey(solutionData.ExecutionId.ToString()), new RedisValue(jobData), TimeSpan.FromMinutes(5));
                return Result<CreateUnverifiedProblemDto, ErrorObject<string>>.Ok(new CreateUnverifiedProblemDto
                {
                    JobId = solutionData.ExecutionId,
                    ProblemId = ok
                });
            }, 
            err => Task.FromResult(Result<CreateUnverifiedProblemDto, ErrorObject<string>>.Err(err)));
    }
}

public class JobData
{
    public required Guid ProblemId { get; set; }
    public required Guid CommissioningUserId { get; set; }
    public List<ValidationResponse> CachedResponses { get; set; } = [];
}