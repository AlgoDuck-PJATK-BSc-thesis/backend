namespace ExecutorService.Executor.Types.FilesystemPoolerTypes;

internal sealed class CacheTargetData(int cacheTargetMin, int cacheTargetInit, int cacheTargetMax)
{
    internal int CacheTargetMin { get; } = cacheTargetMin;
    internal int CacheTargetCurrent { get; set; } = cacheTargetInit;
    internal int CacheTargetMax { get; } = cacheTargetMax;
}