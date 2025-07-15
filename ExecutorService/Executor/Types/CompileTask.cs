namespace ExecutorService.Executor.Types;

public class CompileTask(string code, string classname, TaskCompletionSource<byte[]> tcs)
{
    public string Code => code;
    public string ClassName => classname;
    public TaskCompletionSource<byte[]> Tcs => tcs;
}