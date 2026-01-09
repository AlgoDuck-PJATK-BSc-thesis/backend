using System.Text;
using System.Text.Json;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.CreateProblem;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;
using AlgoDuck.Shared.Extensions;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpdateProblem;

public interface IUpdateProblemRepository
{
    public Task<Result<Guid, ErrorObject<string>>> UpdateProblemAsync(UpsertProblemDto upsertProblemDto, Guid problemId, CancellationToken cancellationToken = default);
}

public class UpdateProblemRepository(
    IAwsS3Client s3Client,
    ApplicationCommandDbContext dbContext
    ) : IUpdateProblemRepository
{
    public async Task<Result<Guid, ErrorObject<string>>> UpdateProblemAsync(UpsertProblemDto upsertProblemDto, Guid problemId, CancellationToken cancellationToken = default)
    {
        var problem = await dbContext.Problems
            .Include(p => p.Tags)
            .Include(p => p.TestCases)
            .FirstOrDefaultAsync(p => p.ProblemId == problemId, cancellationToken);
        
        if (problem == null)
            return Result<Guid, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Problem with id: {problemId} not found"));

        problem.CategoryId =  upsertProblemDto.CategoryId;
        problem.ProblemTitle = upsertProblemDto.ProblemTitle;
        problem.LastUpdatedAt = DateTime.UtcNow;
        problem.DifficultyId = upsertProblemDto.DifficultyId;
        
        problem.Tags.Clear();
        foreach (var tag in upsertProblemDto.Tags)
        {
            problem.Tags.Add(new Tag { TagName = tag.TagName });
        }
        var (s3Partials, newDbTestCases) = BuildTestCases(upsertProblemDto.TestCaseJoins);

        var testCases = newDbTestCases.ToDictionary(tc => tc.TestCaseId, tc => tc);
        
        var idsToUpdate = newDbTestCases.Select(tInner => tInner.TestCaseId).ToHashSet();

        var toRemove = problem.TestCases.Where(t => !idsToUpdate.Contains(t.TestCaseId)).ToList();
        dbContext.TestCases.RemoveRange(toRemove);

        foreach (var problemTestCase in problem.TestCases.Where(t => idsToUpdate.Contains(t.TestCaseId)))
        {
            if (!testCases.TryGetValue(problemTestCase.TestCaseId, out var testCase)) continue;
            problemTestCase.ArrangeVariableCount = testCase.ArrangeVariableCount;
            problemTestCase.CallFunc = testCase.CallFunc;
            problemTestCase.Display = testCase.Display;
            problemTestCase.DisplayRes = testCase.DisplayRes;
            problemTestCase.IsPublic = testCase.IsPublic;
            problemTestCase.OrderMatters = testCase.OrderMatters;
            testCases.Remove(problemTestCase.TestCaseId);
        }

        foreach (var newTestCase in testCases.Values)
        {
            problem.TestCases.Add(newTestCase);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        if (s3Partials.Count > 0)
        {
            await UploadProblemAssetsToS3(problem.ProblemId, upsertProblemDto, s3Partials, cancellationToken);
        }

        return Result<Guid, ErrorObject<string>>.Ok(problem.ProblemId);
    }
    
    private static (List<TestCaseS3Partial> S3, List<TestCase> Db) BuildTestCases(
        List<TestCaseJoined> testCaseJoins)
    {
        var s3TestCases = new List<TestCaseS3Partial>();
        var dbTestCases = new List<TestCase>();

        foreach (var tc in testCaseJoins)
        {
            var testCaseId = tc.TestCaseId;

            s3TestCases.Add(new TestCaseS3Partial
            {
                Call = tc.Call,
                Expected = tc.Expected,
                Setup = tc.Setup,
                TestCaseId = testCaseId
            });

            dbTestCases.Add(new TestCase
            {
                CallFunc = tc.CallFunc,
                Display = tc.Display,
                IsPublic = tc.IsPublic,
                DisplayRes = tc.DisplayRes,
                TestCaseId = testCaseId,
                ArrangeVariableCount = tc.VariableCount
            });
        }

        return (s3TestCases, dbTestCases);
    }

    private async Task UploadProblemAssetsToS3(
        Guid problemId,
        UpsertProblemDto dto,
        List<TestCaseS3Partial> testCases,
        CancellationToken cancellationToken)
    {
        var uploadTasks = new Task[]
        {
            UploadTestCases(problemId, testCases, cancellationToken),
            UploadProblemInfo(problemId, dto, cancellationToken),
            UploadTemplate(problemId, dto.TemplateB64, cancellationToken)
        };

        await Task.WhenAll(uploadTasks);
    }

    private Task<Result<TestCaseS3WrapperObject,ErrorObject<string>>> UploadTestCases(
        Guid problemId,
        List<TestCaseS3Partial> testCases,
        CancellationToken cancellationToken)
    {
        return s3Client.PostXmlObjectAsync(
            S3Paths.TestCases(problemId),
            new TestCaseS3WrapperObject
            {
                ProblemId = problemId,
                TestCases = testCases
            },
            cancellationToken);
    }

    private Task<Result<ProblemS3PartialInfo,ErrorObject<string>>> UploadProblemInfo(
        Guid problemId,
        UpsertProblemDto dto,
        CancellationToken cancellationToken)
    {
        var languageCode = SupportedLanguage.En.GetDisplayName().ToLowerInvariant();

        return s3Client.PostXmlObjectAsync(
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

    private Task<Result<ProblemS3PartialTemplate,ErrorObject<string>>> UploadTemplate(
        Guid problemId,
        string templateB64,
        CancellationToken cancellationToken)
    {
        var templateContent = Encoding.UTF8.GetString(Convert.FromBase64String(templateB64));

        return s3Client.PostXmlObjectAsync(
            S3Paths.Template(problemId),
            new ProblemS3PartialTemplate
            {
                ProblemId = problemId,
                Template = templateContent
            },
            cancellationToken);
    }
}