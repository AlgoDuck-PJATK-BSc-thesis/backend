using System.Text;
using AlgoDuck.DAL;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using AlgoDuck.Shared.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.InsertTestCaseIntoUserCode;

[ApiController]
[Route("api/[controller]")]
public class TestCaseInsertController(
    IInsertService insertService
    ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> InsertTestCaseIntoUserCodeAsync([FromBody] InsertRequestDto insertRequest, CancellationToken cancellationToken)
    {
        return Ok(new StandardApiResponse<InsertResultDto>
        {
            Body = await insertService.InsertTestCaseAsync(insertRequest, cancellationToken)
        });
    }
}

public interface IInsertService
{
    public Task<InsertResultDto> InsertTestCaseAsync(InsertRequestDto insertRequest, CancellationToken cancellationToken);
}

public class InsertService(
    IInsertRepository insertRepository
    ) : IInsertService
{
    public async Task<InsertResultDto> InsertTestCaseAsync(InsertRequestDto insertRequest, CancellationToken cancellationToken)
    {
        return await insertRepository.InsertTestCaseAsync(insertRequest, cancellationToken);
    }
}

public interface IInsertRepository
{
    public Task<InsertResultDto> InsertTestCaseAsync(InsertRequestDto insertRequest, CancellationToken cancellationToken);
}

public class InsertRepository(
    IAwsS3Client awsS3Client,
    ApplicationQueryDbContext dbContext
    ) : IInsertRepository
{
    private async Task<List<TestCaseJoined>> GetTestCasesAsync(Guid exerciseId, CancellationToken cancellationToken)
    {
        var exerciseDbPartialTestCases =
            await dbContext.TestCases.Where(t => t.ProblemProblemId == exerciseId).ToDictionaryAsync(t => t.TestCaseId, t => t, cancellationToken: cancellationToken);

        var exerciseS3PartialTestCases = XmlToObjectParser.ParseXmlString<TestCaseS3WrapperObject>(
                                             await awsS3Client.GetDocumentStringByPathAsync(
                                                 $"problems/{exerciseId}/test-cases.xml", cancellationToken))
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
    
    public async Task<InsertResultDto> InsertTestCaseAsync(InsertRequestDto insertRequest, CancellationToken cancellationToken)
    {
        var userSolutionData = new UserSolutionData
        {
            FileContents = new StringBuilder(Encoding.UTF8.GetString(Convert.FromBase64String(insertRequest.UserCodeB64)))
        };
        
        var analyzer = new AnalyzerSimple(userSolutionData.FileContents);
        userSolutionData.IngestCodeAnalysisResult(analyzer.AnalyzeUserCode(ExecutionStyle.Execution));
        var helper = new ExecutorFileOperationHelper
        {
            UserSolutionData = userSolutionData
        };
        
        var testCases = await GetTestCasesAsync(insertRequest.ExerciseId, cancellationToken);
        var insertedTestCase = testCases
            .FirstOrDefault(t => t.TestCaseId == insertRequest.TestCaseId)
                ?? throw new InvalidOperationException($"Unable to find test case {insertRequest.TestCaseId}");

        helper.ArrangeTestCase(insertedTestCase, userSolutionData.MainClassName);
        helper.ActTestCase(insertedTestCase);

        return new InsertResultDto
        {
            ModifiedCodeB64 = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(userSolutionData.FileContents.ToString())
            )
        };
    }
}



public class InsertRequestDto
{
    public required string UserCodeB64 { get; set; }
    public required Guid ExerciseId { get; set; }
    public required Guid TestCaseId { get; set; }
}

public class InsertResultDto
{
    public required string ModifiedCodeB64 { get; set; }
}