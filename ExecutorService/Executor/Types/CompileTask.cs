using ExecutorService.Executor.Dtos;

namespace ExecutorService.Executor.Types;

public class CompileTask(string code, string classname, TaskCompletionSource<CompileResultDto> tcs)
{
    public string Code => code;
    public string ClassName => classname;
    public TaskCompletionSource<CompileResultDto> Tcs => tcs;
}