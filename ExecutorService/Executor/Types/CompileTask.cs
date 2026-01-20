using AlgoDuckShared;
using ExecutorService.Executor.Types.VmLaunchTypes;
using ExecutorService.Executor.VmLaunchSystem;

namespace ExecutorService.Executor.Types;

internal class CompileTask(VmJobRequestInterface<VmCompilationPayload> request, TaskCompletionSource<VmCompilationResponse> tcs)
{
    internal VmJobRequestInterface<VmCompilationPayload> Request => request;
    internal TaskCompletionSource<VmCompilationResponse> Tcs => tcs;
}