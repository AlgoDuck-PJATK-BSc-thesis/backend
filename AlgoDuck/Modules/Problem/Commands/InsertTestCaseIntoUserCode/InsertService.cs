using System.Text;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Modules.Problem.Shared.Repositories;
using AlgoDuck.Modules.Problem.Shared.Types;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Commands.InsertTestCaseIntoUserCode;

public interface IInsertService
{
    public Task<Result<InsertResultDto, ErrorObject<string>>> InsertTestCaseAsync(InsertRequestDto insertRequest,
        CancellationToken cancellationToken = default);
}

public class InsertService : IInsertService
{
    private readonly ISharedProblemRepository _problemRepository;

    public InsertService(ISharedProblemRepository problemRepository)
    {
        _problemRepository = problemRepository;
    }

    public async Task<Result<InsertResultDto, ErrorObject<string>>> InsertTestCaseAsync(InsertRequestDto insertRequest,
        CancellationToken cancellationToken = default)
    {
        return await _problemRepository
            .GetTestCasesAsync(insertRequest.ExerciseId, cancellationToken)
            .BindResult<ICollection<TestCaseJoined>, InsertResultDto, ErrorObject<string>>(testCases =>
                FindTestCase(testCases, insertRequest.TestCaseId)
                    .Bind<TestCaseJoined, InsertResultDto, ErrorObject<string>>(testCase => InsertActualTestCase(insertRequest, testCase)));
    }


    private static Result<InsertResultDto, ErrorObject<string>> InsertActualTestCase(InsertRequestDto insertRequest, TestCaseJoined testCaseJoined)
    {
        var userSolutionData = new UserSolutionData
        {
            FileContents = new StringBuilder(Encoding.UTF8.GetString(Convert.FromBase64String(insertRequest.UserCodeB64)))
        };
        try
        {
            var analyzer = new AnalyzerSimple(userSolutionData.FileContents);
            var codeAnalysisResult = analyzer.AnalyzeUserCode(ExecutionStyle.Execution);
            userSolutionData.IngestCodeAnalysisResult(codeAnalysisResult);
            var helper = new ExecutorFileOperationHelper
            {
                UserSolutionData = userSolutionData
            };
            helper.InsertTestCaseForExecution(codeAnalysisResult, testCaseJoined);

            return Result<InsertResultDto, ErrorObject<string>>.Ok(new InsertResultDto
            {
                ModifiedCodeB64 =
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(userSolutionData.FileContents.ToString()))
            });
        }
        catch (JavaSyntaxException ex)
        {
            return Result<InsertResultDto, ErrorObject<string>>.Err(
                ErrorObject<string>.BadRequest($"Could not insert test case: Reason: {ex.Message}"));
        }
        catch (Exception)
        {
            return Result<InsertResultDto, ErrorObject<string>>.Err(
                ErrorObject<string>.InternalError("Could not insert test case"));
        }
    }

    private static Result<TestCaseJoined, ErrorObject<string>> FindTestCase(ICollection<TestCaseJoined> testCases,
        Guid toBeFound)
    {
        var testCase = testCases.FirstOrDefault(t => t.TestCaseId == toBeFound);
        if (testCase == null)
            return Result<TestCaseJoined, ErrorObject<string>>.Err(
                ErrorObject<string>.NotFound($"test case: '{toBeFound}' not found"));

        return Result<TestCaseJoined, ErrorObject<string>>.Ok(testCase);
    }
}