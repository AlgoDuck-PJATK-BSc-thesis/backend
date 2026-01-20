namespace ExecutorService.Executor.Types.Config;

public class CompilationHandlerConfig
{
    public required int DefaultCompilerCount { get; set; }
    public required int WorkerThreadCount { get; set; }
    public required int PollingFrequencyMs { get; set; }
}