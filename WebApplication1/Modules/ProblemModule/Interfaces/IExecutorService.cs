using ExecutorService.Executor._ExecutorUtils;

namespace WebApplication1.Modules.ProblemModule.Interfaces;

public interface IExecutorService
{
    public Task<ExecuteResultDto> DryExecuteCode(ExecuteRequestDto executeRequest);   
    public Task<ExecuteResultDto> FullExecuteCode(ExecuteRequestDto executeRequest);   
}