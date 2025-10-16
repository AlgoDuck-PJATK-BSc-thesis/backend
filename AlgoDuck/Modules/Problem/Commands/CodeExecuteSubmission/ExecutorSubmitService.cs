using AlgoDuck.Modules.Problem.ExecutorShared;
using AlgoDuckShared.Executor.SharedTypes;

namespace AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;


public interface IExecutorSubmitService
{
    internal Task<ExecuteResponse> SubmitUserCode(SubmitExecuteRequest submission);
}

internal class ExecutorSubmitService(IExecutorQueryInterface executorQueryInterface) : IExecutorSubmitService
{
    
    public Task<ExecuteResponse> SubmitUserCode(SubmitExecuteRequest submission)
    {
        return executorQueryInterface.ExecuteAsync(submission);
    }
}
