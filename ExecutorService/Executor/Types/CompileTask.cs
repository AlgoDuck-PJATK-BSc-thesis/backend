using ExecutorService.Executor.Types.VmLaunchTypes;
using ExecutorService.Executor.VmLaunchSystem;

namespace ExecutorService.Executor.Types;

internal class CompileTask(UserSolutionData userSolutionData, TaskCompletionSource<VmCompilationResponse> tcs)
{
    internal UserSolutionData UserSolutionData => userSolutionData;
    internal TaskCompletionSource<VmCompilationResponse> Tcs => tcs;
}