using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.Types.VmLaunchTypes;
using ExecutorService.Executor.VmLaunchSystem;

namespace ExecutorService.Executor;

public interface ICodeExecutorService
{
    public Task<VmExecutionResponse> Execute(ExecutionRequest request);
}

internal class CodeExecutorService(
    ICompilationHandler compilationHandler,
    VmLaunchManager launchManager
    ) : ICodeExecutorService
{
    public async Task<VmExecutionResponse> Execute(ExecutionRequest request)
    {
        var vmLeaseTask = launchManager.AcquireVmAsync(FilesystemType.Executor); 
        var compilationResult = await compilationHandler.CompileAsync(request);
        using var vmLease = await vmLeaseTask;
        if (compilationResult is VmCompilationFailure failure)
        {
            throw new CompilationException(failure.ErrorMsg);
        }
        
        return await vmLease.QueryAsync<VmExecutionQuery, VmExecutionResponse>(new VmExecutionQuery((compilationResult as VmCompilationSuccess)!));
    }
}
