namespace ExecutorService.Executor.Types.Config;

public class FileSystemPoolerConfig
{
    public required long TrackingPeriodMinutes { get; init; }
    public required long PollingFrequencySeconds { get; init; }
    public required long DefaultCompilerCacheTarget { get; init; }
    public required long DefaultExecutorCacheTarget { get; init; }
}