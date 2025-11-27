using System.Text;
using AlgoDuck.Modules.Problem.ExecutorShared;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;
using AlgoDuckShared.Executor.SharedTypes;

namespace AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;


public interface IExecutorDryRunService
{
    internal Task<ExecuteResponse> DryRunUserCodeAsync(DryExecuteRequest request);
}

internal class DryRunService(
    IExecutorQueryInterface executorQueryInterface
    ) : IExecutorDryRunService
{
    
    public async Task<ExecuteResponse> DryRunUserCodeAsync(DryExecuteRequest request)
    {
        var userSolutionData = new UserSolutionData
        {
            FileContents = new StringBuilder(Encoding.UTF8.GetString(Convert.FromBase64String(request.CodeB64)))
        };
        var analyzer = new AnalyzerSimple(userSolutionData.FileContents);
        userSolutionData.IngestCodeAnalysisResult(analyzer.AnalyzeUserCode(ExecutionStyle.Execution));
        
        var helper = new ExecutorFileOperationHelper(userSolutionData);

        helper.InsertTiming();
        
        var executionResponse = await executorQueryInterface.ExecuteAsync(new ExecutionRequest
        {
            JavaFiles = userSolutionData.GetFileContents()
        });
        
        return helper.ParseVmOutput(executionResponse);
    }
}
