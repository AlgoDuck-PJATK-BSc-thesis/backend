using ExecutorService.Executor._ExecutorUtils;
using WebApplication1.Modules.ProblemModule.DTOs;
using WebApplication1.Modules.ProblemModule.DTOs.ExecutorDtos;

namespace WebApplication1.Modules.ProblemModule.Interfaces;

public interface IExecutorService
{
    public Task<ExecuteResultDto> DryExecuteCode(DryExecuteRequestDto executeRequest);   
    public Task<ExecuteResultDto> FullExecuteCode(ExecuteRequestDto executeRequest);   
}