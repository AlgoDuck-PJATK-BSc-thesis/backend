using System.Text;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.CreateProblem;

public interface ICreateProblemRepository
{
    Task<Result<Guid, ErrorObject<string>>> CreateProblemAsync(
        UpsertProblemDto problemDto,
        CancellationToken cancellationToken = default);

    Task<Result<Guid, ErrorObject<string>>> ConfirmProblemUponValidationSuccessAsync(
        Guid problemId,
        CancellationToken cancellationToken = default);

    Task<Result<Guid, ErrorObject<string>>> DeleteProblemUponValidationFailureAsync(
        Guid problemId,
        CancellationToken cancellationToken = default);
}

public class CreateProblemRepository : ICreateProblemRepository
{
    private readonly ApplicationCommandDbContext _dbContext;
    private readonly IAwsS3Client _s3Client;

    public CreateProblemRepository(ApplicationCommandDbContext dbContext, IAwsS3Client s3Client)
    {
        _dbContext = dbContext;
        _s3Client = s3Client;
    }

    public async Task<Result<Guid, ErrorObject<string>>> CreateProblemAsync(
        UpsertProblemDto problemDto,
        CancellationToken cancellationToken = default)
    {
        var (s3TestCases, dbTestCases) = BuildTestCases(problemDto.TestCaseJoins);
        var problem = BuildProblemEntity(problemDto, dbTestCases);

        await _dbContext.Problems.AddAsync(problem, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await UploadProblemAssetsToS3(problem.ProblemId, problemDto, s3TestCases, cancellationToken);

        return Result<Guid, ErrorObject<string>>.Ok(problem.ProblemId);
    }

    public async Task<Result<Guid, ErrorObject<string>>> DeleteProblemUponValidationFailureAsync(
        Guid problemId,
        CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _dbContext.Problems
            .Where(p => p.ProblemId == problemId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (rowsAffected == 0)
        {
            return Result<Guid, ErrorObject<string>>.Err(
                ErrorObject<string>.NotFound($"Problem with id: {problemId} not found. Failed to delete"));
        }

        var deletionResult = await _s3Client.DeleteAllByPrefixAsync(
            S3Paths.ProblemPrefix(problemId),
            cancellationToken: cancellationToken);

        return deletionResult.IsErr
            ? Result<Guid, ErrorObject<string>>.Err(deletionResult.AsErr!)
            : Result<Guid, ErrorObject<string>>.Ok(problemId);
    }

    public async Task<Result<Guid, ErrorObject<string>>> ConfirmProblemUponValidationSuccessAsync(
        Guid problemId,
        CancellationToken cancellationToken = default)
    {
        var rowsUpdated = await _dbContext.Problems
            .Where(p => p.ProblemId == problemId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(p => p.Status, ProblemStatus.Verified),
                cancellationToken);
        if (rowsUpdated == 0)
        {
            return Result<Guid, ErrorObject<string>>.Err(
                ErrorObject<string>.NotFound($"Problem with id: {problemId} not found. Failed to verify"));
        }

        return Result<Guid, ErrorObject<string>>.Ok(problemId);
    }

    private static (List<TestCaseS3Partial> S3, List<TestCase> Db) BuildTestCases(
        List<TestCaseJoined> testCaseJoins)
    {
        var s3TestCases = new List<TestCaseS3Partial>();
        var dbTestCases = new List<TestCase>();

        foreach (var tc in testCaseJoins)
        {
            var testCaseId = Guid.NewGuid();

            s3TestCases.Add(new TestCaseS3Partial
            {
                Call = tc.Call,
                Expected = tc.Expected,
                Setup = tc.Setup,
                TestCaseId = testCaseId
            });

            dbTestCases.Add(new TestCase
            {
                CallFunc = tc.CallFunc ?? "",
                Display = tc.Display,
                IsPublic = tc.IsPublic,
                OrderMatters = tc.OrderMatters,
                DisplayRes = tc.DisplayRes,
                TestCaseId = testCaseId,
                ArrangeVariableCount = tc.VariableCount,
                InPlace = tc.InPlace
            });
        }

        return (s3TestCases, dbTestCases);
    }

    private static Models.Problem BuildProblemEntity(UpsertProblemDto dto, List<TestCase> testCases)
    {
        return new Models.Problem
        {
            ProblemTitle = dto.ProblemTitle,
            CreatedAt = DateTime.UtcNow,
            DifficultyId = dto.DifficultyId,
            TestCases = testCases,
            CreatedByUserId = dto.RequestingUserId,
            CategoryId = dto.CategoryId,
            Tags = dto.Tags.Select(t => new Tag { TagName = t.TagName }).ToList()
        };
    }

    private async Task UploadProblemAssetsToS3(
        Guid problemId,
        UpsertProblemDto dto,
        List<TestCaseS3Partial> testCases,
        CancellationToken cancellationToken)
    {
        var uploadTasks = new[]
        {
            UploadTestCases(problemId, testCases, cancellationToken),
            UploadProblemInfo(problemId, dto, cancellationToken),
            UploadTemplate(problemId, dto.TemplateB64, cancellationToken)
        };

        await Task.WhenAll(uploadTasks);
    }

    private Task UploadTestCases(
        Guid problemId,
        List<TestCaseS3Partial> testCases,
        CancellationToken cancellationToken)
    {
        return _s3Client.PostXmlObjectAsync(
            S3Paths.TestCases(problemId),
            new TestCaseS3WrapperObject
            {
                ProblemId = problemId,
                TestCases = testCases
            },
            cancellationToken);
    }

    private Task UploadProblemInfo(
        Guid problemId,
        UpsertProblemDto dto,
        CancellationToken cancellationToken)
    {
        var languageCode = SupportedLanguage.En.GetDisplayName().ToLowerInvariant();

        return _s3Client.PostXmlObjectAsync(
            S3Paths.ProblemInfo(problemId, languageCode),
            new ProblemS3PartialInfo
            {
                ProblemId = problemId,
                CountryCode = SupportedLanguage.En,
                Description = dto.ProblemDescription,
                Title = dto.ProblemTitle
            },
            cancellationToken);
    }

    private Task UploadTemplate(
        Guid problemId,
        string templateB64,
        CancellationToken cancellationToken)
    {
        var templateContent = Encoding.UTF8.GetString(Convert.FromBase64String(templateB64));

        return _s3Client.PostXmlObjectAsync(
            S3Paths.Template(problemId),
            new ProblemS3PartialTemplate
            {
                ProblemId = problemId,
                Template = templateContent
            },
            cancellationToken);
    }
}

internal static class S3Paths
{
    public static string ProblemPrefix(Guid problemId) => $"problems/{problemId}";

    public static string TestCases(Guid problemId) => $"problems/{problemId}/test-cases.xml";

    public static string ProblemInfo(Guid problemId, string languageCode) =>
        $"problems/{problemId}/infos/{languageCode}.xml";

    public static string Template(Guid problemId) => $"problems/{problemId}/template.xml";
}