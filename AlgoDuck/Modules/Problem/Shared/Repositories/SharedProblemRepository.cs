using AlgoDuck.DAL;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Queries.GetProblemDetailsByName;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using IAwsS3Client = AlgoDuck.Shared.S3.IAwsS3Client;

namespace AlgoDuck.Modules.Problem.Shared.Repositories;

public interface ISharedProblemRepository
{
    public Task<Result<ProblemDto, ErrorObject<string>>> GetProblemDetailsAsync(Guid problemId, CancellationToken cancellationToken = default);

    public Task<Result<ICollection<TestCaseJoined>, ErrorObject<string>>> GetTestCasesAsync(Guid exerciseId,
        CancellationToken cancellationToken = default);

    public Task<Result<ProblemS3PartialInfo, ErrorObject<string>>> GetProblemInfoAsync(Guid problemId, SupportedLanguage lang = SupportedLanguage.En,
        CancellationToken cancellationToken = default);

    public Task<Result<ProblemS3PartialTemplate, ErrorObject<string>>> GetTemplateAsync(Guid exerciseId,
        CancellationToken cancellationToken = default);

}

public class SharedProblemRepository(
    ApplicationQueryDbContext dbContext,
    IAwsS3Client awsS3Client
    ) : ISharedProblemRepository
{
    public async Task<Result<ProblemDto, ErrorObject<string>>> GetProblemDetailsAsync(Guid problemId, CancellationToken cancellationToken = default)
    {
        var problemTemplateResult = await GetTemplateAsync(problemId, cancellationToken);
        var testCasesResult = await GetTestCasesAsync(problemId, cancellationToken);
        var problemInfosResult = await GetProblemInfoAsync(problemId, cancellationToken: cancellationToken);
        
        if (problemInfosResult.IsErr)
            return Result<ProblemDto, ErrorObject<string>>.Err(problemInfosResult.AsT1);
        
        if (testCasesResult.IsErr)
            return Result<ProblemDto, ErrorObject<string>>.Err(testCasesResult.AsT1);

        if (problemInfosResult.IsErr)
            return Result<ProblemDto, ErrorObject<string>>.Err(problemInfosResult.AsT1);

        var problem = await dbContext.Problems
            .Include(p => p.Category)
            .Include(p => p.Difficulty)
            .FirstOrDefaultAsync(p => p.ProblemId == problemId, cancellationToken: cancellationToken);
    

        if (problem == null)
            return Result<ProblemDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Problem {problemId} not found"));
        
        var testCases = testCasesResult.AsT0;
        var problemTemplate = problemTemplateResult.AsT0;
        var problemInfo = problemInfosResult.AsT0;
        
        return Result<ProblemDto, ErrorObject<string>>.Ok(new ProblemDto
        {
            Description = problemInfo.Description,
            ProblemId = problem.ProblemId,
            TemplateContents = problemTemplate.Template,
            Title = problemInfo.Title,
            TestCases = testCases.Select(t => new TestCaseDto
            {
                Display = t.IsPublic ? t.Display : "",
                DisplayRes = t.IsPublic ? t.DisplayRes : "",
                IsPublic = t.IsPublic,
                TestCaseId = t.TestCaseId,
            }),
            Category = new CategoryDto
            {
                Name = problem.Category.CategoryName
            },
            Difficulty = new DifficultyDto
            {
                Name = problem.Difficulty.DifficultyName
            }
        });
    }
    
    public async Task<Result<ProblemS3PartialTemplate, ErrorObject<string>>> GetTemplateAsync(Guid exerciseId, CancellationToken cancellationToken = default)
    {
        var templatePath = $"problems/{exerciseId}/template.xml";
        return await awsS3Client.GetXmlObjectByPathAsync<ProblemS3PartialTemplate>(templatePath, cancellationToken);
    }

    public async Task<Result<ProblemS3PartialInfo, ErrorObject<string>>> GetProblemInfoAsync(
        Guid problemId,
        SupportedLanguage lang = SupportedLanguage.En,
        CancellationToken cancellationToken = default)
    {
        var objectPath = $"problems/{problemId}/infos/{lang.GetDisplayName().ToLowerInvariant()}.xml";
        return await awsS3Client.GetXmlObjectByPathAsync<ProblemS3PartialInfo>(objectPath, cancellationToken);
    }
    
    public async Task<Result<ICollection<TestCaseJoined>, ErrorObject<string>>> GetTestCasesAsync(Guid exerciseId, CancellationToken cancellationToken = default)
    {
        var exerciseDbPartialTestCases =
            await dbContext.TestCases.Where(t => t.ProblemProblemId == exerciseId)
                .ToDictionaryAsync(t => t.TestCaseId, t => t, cancellationToken: cancellationToken);

        var testCasesPath = $"problems/{exerciseId}/test-cases.xml";

        var testCaseRes =  await awsS3Client.GetXmlObjectByPathAsync<TestCaseS3WrapperObject>(testCasesPath, cancellationToken);

        if (testCaseRes.IsErr)
            return Result<ICollection<TestCaseJoined>, ErrorObject<string>>.Err(testCaseRes.AsT1);

        return Result<ICollection<TestCaseJoined>, ErrorObject<string>>.Ok(testCaseRes.AsT0.TestCases.Select(t => new
        {
            dbTestCase = exerciseDbPartialTestCases[t.TestCaseId],
            S3TestCase = t
        }).Select(t => new TestCaseJoined
        {
            Call = t.S3TestCase.Call,
            CallFunc = t.dbTestCase.CallFunc,
            VariableCount = t.dbTestCase.ArrangeVariableCount,
            Display = t.dbTestCase.Display,
            DisplayRes = t.dbTestCase.DisplayRes,
            Expected = t.S3TestCase.Expected,
            IsPublic = t.dbTestCase.IsPublic,
            ProblemProblemId = exerciseId,
            Setup = t.S3TestCase.Setup,
            TestCaseId = t.dbTestCase.TestCaseId
        }).ToList());

    }
}