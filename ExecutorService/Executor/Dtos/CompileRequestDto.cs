namespace ExecutorService.Executor.Dtos;

public class CompileRequestDto(string srcCodeB64, string classname)
{
    public string SrcCodeB64 { get; set; } = srcCodeB64;
    public string ClassName { get; set; } = classname;
}