using AlgoDuck.DAL;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Utilities;
using AlgoDuckShared;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using IAwsS3Client = AlgoDuck.Shared.S3.IAwsS3Client;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemDetailsByName;

public interface IProblemRepository
{
    public Task<ProblemDto> GetProblemDetailsAsync(Guid problemId);
}

public class ProblemRepository(
    ApplicationQueryDbContext dbContext,
    IAwsS3Client awsS3Client
    ) : IProblemRepository
{
    public async Task<ProblemDto> GetProblemDetailsAsync(Guid problemId)
    {
        var problemTemplate = await GetTemplateAsync(problemId);
        var testCases = await GetTestCasesAsync(problemId);
        var problemInfos = await GetProblemInfoAsync(problemId);
    
        var problem = await dbContext.Problems
                          .Include(p => p.Category)
                          .Include(p => p.Difficulty)
                          .FirstOrDefaultAsync(p => p.ProblemId == problemId)
                      ?? throw new NotFoundException($"Problem {problemId} not found");
    
        return new ProblemDto
        {
            Description = problemInfos.Description,
            ProblemId = problem.ProblemId,
            TemplateContents = problemTemplate.Template,
            Title = problemInfos.Title,
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
        };
    }
    
    /*TODO: Since these repeat in the submit command perhaps we could extract them to a separate TestCaseRepo/TemplateRepo?*/
    private async Task<ProblemS3PartialTemplate> GetTemplateAsync(Guid exerciseId)
    {
        return XmlToObjectParser.ParseXmlString<ProblemS3PartialTemplate>(
            await awsS3Client.GetDocumentStringByPathAsync($"problems/{exerciseId}/template.xml")
        ) ?? throw new XmlParsingException();
    }

    private async Task<ProblemS3PartialInfo> GetProblemInfoAsync(
        Guid problemId,
        SupportedLanguage lang = SupportedLanguage.En)
    {
        var objectPath = $"problems/{problemId}/infos/{lang.GetDisplayName().ToLowerInvariant()}.xml";
        if (!await awsS3Client.ObjectExistsAsync(objectPath))
        {
            throw new NotFoundException(objectPath);
        }

        var problemInfosRaw = await awsS3Client.GetDocumentStringByPathAsync(objectPath);
        var problemInfos = XmlToObjectParser.ParseXmlString<ProblemS3PartialInfo>(problemInfosRaw)
            ?? throw new XmlParsingException(objectPath);
        
        return problemInfos;
    }
    
    private async Task<List<TestCaseJoined>> GetTestCasesAsync(Guid exerciseId)
    {
        var exerciseDbPartialTestCases =
            await dbContext.TestCases.Where(t => t.ProblemProblemId == exerciseId)
                .ToDictionaryAsync(t => t.TestCaseId, t => t);

        var exerciseS3PartialTestCases = XmlToObjectParser.ParseXmlString<TestCaseS3WrapperObject>(
                                             await awsS3Client.GetDocumentStringByPathAsync(
                                                 $"problems/{exerciseId}/test-cases.xml"))
                                         ?? throw new XmlParsingException($"problems/{exerciseId}/test-cases.xml");
        
        return exerciseS3PartialTestCases.TestCases.Select(t => new
        {
            dbTestCase = exerciseDbPartialTestCases[t.TestCaseId],
            S3TestCase = t
        }).Select(t => new TestCaseJoined
        {
            Call = t.S3TestCase.Call,
            CallFunc = t.dbTestCase.CallFunc,
            Display = t.dbTestCase.Display,
            DisplayRes = t.dbTestCase.DisplayRes,
            Expected = t.S3TestCase.Expected,
            IsPublic = t.dbTestCase.IsPublic,
            ProblemProblemId = exerciseId,
            Setup = t.S3TestCase.Setup,
            TestCaseId = t.dbTestCase.TestCaseId
        }).ToList();
        
    }
}