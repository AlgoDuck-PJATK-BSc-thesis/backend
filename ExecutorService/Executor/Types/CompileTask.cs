namespace ExecutorService.Executor.Types;

public class CompileTask(UserSolutionData userSolutionData, TaskCompletionSource tcs)
{
    internal UserSolutionData UserSolutionData => userSolutionData;
    internal TaskCompletionSource Tcs => tcs;
}