using System.Text;
using System.Text.Json;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertUtils;
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

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.CreateProblem;

public class CreateProblemService : ICreateProblemService
{
    private readonly IRabbitMqChannelFactory _channelFactory;
    private readonly ITestCaseProcessor _testCaseProcessor;
    private readonly IValidationJobPublisher _validationJobPublisher;
    private readonly ICreateProblemRepository _createProblemRepository;
    private readonly IOptions<MessageQueuesConfig> _channelData;
    private readonly IDatabase _redis;

    public CreateProblemService(IRabbitMqChannelFactory channelFactory, ITestCaseProcessor testCaseProcessor,
        IValidationJobPublisher validationJobPublisher, ICreateProblemRepository createProblemRepository,
        IOptions<MessageQueuesConfig> channelData, IDatabase redis)
    {
        _channelFactory = channelFactory;
        _testCaseProcessor = testCaseProcessor;
        _validationJobPublisher = validationJobPublisher;
        _createProblemRepository = createProblemRepository;
        _channelData = channelData;
        _redis = redis;
    }


    public async Task<Result<UpsertProblemResultDto, ErrorObject<string>>> CreateProblemAsync(
        UpsertProblemDto problemDto,
        CancellationToken cancellationToken = default)
    {
        var analysisResult = AnalyzeTemplate(problemDto.TemplateB64);
        if (analysisResult.IsErr)
            return Result<UpsertProblemResultDto, ErrorObject<string>>.Err(analysisResult.AsErr!);

        var (solutionData, analyzer, codeAnalysis) = analysisResult.AsT0;

        var testCaseResult = _testCaseProcessor.ProcessTestCases(
            problemDto.TestCases,
            analyzer,
            codeAnalysis);

        if (testCaseResult.IsErr)
            return Result<UpsertProblemResultDto, ErrorObject<string>>.Err(testCaseResult.AsErr!);

        problemDto.TestCaseJoins = testCaseResult.AsT0;

        var helper = new ExecutorFileOperationHelper { UserSolutionData = solutionData };
        helper.InsertTestCases(problemDto.TestCaseJoins, codeAnalysis);


        var channel = await _channelFactory.GetChannelAsync(new ChannelDeclareDto
            {
                ChannelName = _channelData.Value.Validation.Read,
                QueueDeclareOptions = new QueueDeclareOptions(Durable: true, Exclusive: false, AutoDelete: false)
            },
            new ChannelDeclareDto
            {
                ChannelName = _channelData.Value.Validation.Write,
                QueueDeclareOptions = new QueueDeclareOptions(Durable: true, Exclusive: false, AutoDelete: false)
            });

        await _validationJobPublisher.PublishAsync(channel, solutionData, cancellationToken);

        return await PersistProblemAndCacheJob(problemDto, solutionData, cancellationToken);
    }

    private Result<(UserSolutionData, AnalyzerSimple, CodeAnalysisResult), ErrorObject<string>> AnalyzeTemplate(
        string templateB64)
    {
        try
        {
            var templateContent = Encoding.UTF8.GetString(Convert.FromBase64String(templateB64));
            var solutionData = new UserSolutionData
            {
                FileContents = new StringBuilder(templateContent)
            };

            var analyzer = new AnalyzerSimple(solutionData.FileContents);
            var analysisResult = analyzer.AnalyzeUserCode(ExecutionStyle.Execution);
            solutionData.IngestCodeAnalysisResult(analysisResult);

            return Result<(UserSolutionData, AnalyzerSimple, CodeAnalysisResult), ErrorObject<string>>
                .Ok((solutionData, analyzer, analysisResult));
        }
        catch (JavaSyntaxException)
        {
            return Result<(UserSolutionData, AnalyzerSimple, CodeAnalysisResult), ErrorObject<string>>
                .Err(ErrorObject<string>.ValidationError("Cannot parse template"));
        }
    }

    private async Task<Result<UpsertProblemResultDto, ErrorObject<string>>> PersistProblemAndCacheJob(
        UpsertProblemDto problemDto,
        UserSolutionData solutionData,
        CancellationToken cancellationToken)
    {
        return await _createProblemRepository
            .CreateProblemAsync(problemDto, cancellationToken)
            .BindAsync(async problemId =>
            {
                var jobData = new JobData<UpsertProblemDto>
                {
                    CommissioningUserId = problemDto.RequestingUserId,
                    ProblemId = problemId,
                    UpsertJobType = UpsertJobType.Insert
                };

                await _redis.StringSetAsync(
                    new RedisKey(solutionData.ExecutionId.ToString()),
                    new RedisValue(JsonSerializer.Serialize(jobData)),
                    TimeSpan.FromMinutes(5));

                return Result<UpsertProblemResultDto, ErrorObject<string>>.Ok(
                    new UpsertProblemResultDto
                    {
                        JobId = solutionData.ExecutionId,
                        ProblemId = problemId
                    });
            });
    }
}

public interface ICreateProblemService
{
    Task<Result<UpsertProblemResultDto, ErrorObject<string>>> CreateProblemAsync(
        UpsertProblemDto problemDto,
        CancellationToken cancellationToken = default);
}