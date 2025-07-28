namespace ExecutorService.Executor.Configs;

public class CompilationHandler 
{
    public int BASE_PORT { get; set; }
    public int BASE_COUNT { get; set; }
    public int THREAD_COUNT { get; set; }
    public CompilerConfig COMPILER_CONFIG { get; set; }
}