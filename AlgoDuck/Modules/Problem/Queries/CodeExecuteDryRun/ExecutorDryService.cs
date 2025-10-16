using AlgoDuck.Modules.Problem.ExecutorShared;
using AlgoDuckShared.Executor.SharedTypes;

namespace AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;


public interface IExecutorDryService
{
    internal Task<ExecuteResponse> DryRunUserCode(DryExecuteRequest submission);
}


internal class ExecutorDryService(IExecutorQueryInterface executorQueryInterface) : IExecutorDryService
{
    public Task<ExecuteResponse> DryRunUserCode(DryExecuteRequest submission)
    {
        return executorQueryInterface.ExecuteAsync(submission);
    }
}