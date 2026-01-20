namespace ExecutorService.Executor.Types.FilesystemPoolerTypes;

internal sealed class RequestProcessingData(Guid filesystemId, bool isCached)
{
    internal Guid FilesystemId { get; } = filesystemId;
    internal bool IsCached { get; } = isCached;
}
