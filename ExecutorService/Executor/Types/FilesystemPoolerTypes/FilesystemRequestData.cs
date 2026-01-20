namespace ExecutorService.Executor.Types.FilesystemPoolerTypes;

internal sealed class FilesystemRequestData(DateTime requestDate, bool isCached)
{
    internal DateTime RequestDate { get; } = requestDate;
    internal bool IsCached { get; } = isCached;
}
