using ExecutorService.Executor.Types.VmLaunchTypes;
using ExecutorService.Executor.VmLaunchSystem;

namespace ExecutorService.Executor.Types;

internal class CompileTask(ExecutionRequest request, TaskCompletionSource<VmCompilationResponse> tcs)
{
    internal ExecutionRequest Request => request;
    internal TaskCompletionSource<VmCompilationResponse> Tcs => tcs;
}