using System.Text;
using System.Text.Json;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.CreateProblem;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertUtils;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Modules.Problem.Shared.Types;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;
using AlgoDuck.Shared.Extensions;
using AlgoDuck.Shared.Http;
using AlgoDuckShared;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpdateProblem;

public interface IUpdateProblemService
{
    public Task<Result<UpsertProblemResultDto, ErrorObject<string>>> UpdateProblemAsync(UpsertProblemDto updateProblemDto, Guid problemId, CancellationToken cancellationToken = default);
}

public class UpdateProblemService(
    IRabbitMqChannelFactory channelFactory,
    ITestCaseProcessor testCaseProcessor,
    IValidationJobPublisher validationJobPublisher,
    IOptions<MessageQueuesConfig> channelData,
    IDatabase redis
    ) : IUpdateProblemService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions().WithInternalFields();

    public async Task<Result<UpsertProblemResultDto, ErrorObject<string>>> UpdateProblemAsync(UpsertProblemDto updateProblemDto, Guid problemId, CancellationToken cancellationToken = default)
    {
        var analysisResult = AnalyzeTemplate(updateProblemDto.TemplateB64);
        if (analysisResult.IsErr)
            return Result<UpsertProblemResultDto, ErrorObject<string>>.Err(analysisResult.AsErr!);

        var (solutionData, analyzer, codeAnalysis) = analysisResult.AsT0;
        
        var testCaseResult = testCaseProcessor.ProcessTestCases(
            updateProblemDto.TestCases,
            analyzer,
            codeAnalysis);
        
        if (testCaseResult.IsErr)
            return Result<UpsertProblemResultDto, ErrorObject<string>>.Err(testCaseResult.AsErr!);

        updateProblemDto.TestCaseJoins = testCaseResult.AsT0;

        var helper = new ExecutorFileOperationHelper { UserSolutionData = solutionData };
        
        var res = await CacheUpdateJob(updateProblemDto, solutionData, problemId, cancellationToken);
        
        helper.InsertTestCases(updateProblemDto.TestCaseJoins, codeAnalysis);

        var channel = await channelFactory.GetChannelAsync(new ChannelDeclareDto
        {
            ChannelName = channelData.Value.Validation.Read,
            QueueDeclareOptions = new QueueDeclareOptions(Durable: true, Exclusive: false, AutoDelete: false)
        },
        new ChannelDeclareDto
        {
            ChannelName = channelData.Value.Validation.Write,
            QueueDeclareOptions = new QueueDeclareOptions(Durable: true, Exclusive: false, AutoDelete: false)
        });
        
        await validationJobPublisher.PublishAsync(channel, solutionData, cancellationToken);
        return res;
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

    private async Task<Result<UpsertProblemResultDto, ErrorObject<string>>> CacheUpdateJob(
        UpsertProblemDto problemDto,
        UserSolutionData solutionData,
        Guid problemId,
        CancellationToken cancellationToken = default)
    {
        var jobData = new JobData<UpsertProblemDto>
        {
            CommissioningUserId = problemDto.RequestingUserId,
            ProblemId = problemId,
            UpsertJobType = UpsertJobType.Update,
            JobBody = problemDto
        };

        await redis.StringSetAsync(
            new RedisKey(solutionData.ExecutionId.ToString()),
            new RedisValue(JsonSerializer.Serialize(jobData, _jsonSerializerOptions)),
            TimeSpan.FromMinutes(5));

        return Result<UpsertProblemResultDto, ErrorObject<string>>.Ok(
            new UpsertProblemResultDto
            {
                JobId = solutionData.ExecutionId,
                ProblemId = problemId
            });
    }
}