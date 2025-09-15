using AlgoDuck.Modules.Problem.DTOs.ExecutorDtos;
using AlgoDuck.Modules.Problem.DTOs;

namespace AlgoDuck.Modules.Problem.Interfaces;

public interface IExecutorService
{
    public Task<ExecuteResultDto> DryExecuteCode(DryExecuteRequestDto executeRequest);   
    public Task<ExecuteResultDto> FullExecuteCode(ExecuteRequestDto executeRequest);   
}