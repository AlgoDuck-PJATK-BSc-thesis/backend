using System.Text;
using AlgoDuck.Modules.Problem.ExecutorShared;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;
using AlgoDuckShared.Executor.SharedTypes;

namespace AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;


public interface IExecutorSubmitService
{
    internal Task<ExecuteResponse> SubmitUserCodeAsync(SubmitExecuteRequest submission);
}


internal class SubmitService(
    IExecutorSubmitRepository executorSubmitRepository,
    IExecutorQueryInterface executorQueryInterface
    ) : IExecutorSubmitService
{
    public async Task<ExecuteResponse> SubmitUserCodeAsync(SubmitExecuteRequest submission)
    {
        var userSolutionData = new UserSolutionData
        {
            FileContents = new StringBuilder(Encoding.UTF8.GetString(Convert.FromBase64String(submission.CodeB64)))
        };
        
        var exerciseTemplate = await executorSubmitRepository.GetTemplateAsync(submission.ExerciseId);
        
        var analyzer = new AnalyzerSimple(userSolutionData.FileContents, exerciseTemplate.Template);
        userSolutionData.IngestCodeAnalysisResult(analyzer.AnalyzeUserCode(ExecutionStyle.Submission));
        
        var helper = new ExecutorFileOperationHelper(userSolutionData);
        
        helper.InsertTestCases(await executorSubmitRepository.GetTestCasesAsync(submission.ExerciseId), userSolutionData.MainClassName);
        helper.InsertTiming();
        helper.InsertGsonImport();

        var executionResponse = await executorQueryInterface.ExecuteAsync(new ExecutionRequest
        {
            JavaFiles = userSolutionData.GetFileContents()
        });

        return helper.ParseVmOutput(executionResponse);
    }
}



