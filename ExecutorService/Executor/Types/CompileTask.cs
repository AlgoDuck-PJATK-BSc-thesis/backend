using AlgoDuckShared;
using ExecutorService.Executor.Types.VmLaunchTypes;
using ExecutorService.Executor.VmLaunchSystem;

namespace ExecutorService.Executor.Types;

internal class CompileTask(SubmitExecuteRequestRabbit request, TaskCompletionSource<VmCompilationResponse> tcs)
{
    internal SubmitExecuteRequestRabbit Request => request;
    internal TaskCompletionSource<VmCompilationResponse> Tcs => tcs;
}