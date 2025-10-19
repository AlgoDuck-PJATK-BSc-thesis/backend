using ExecutorService.Executor.ResourceHandlers;

namespace ExecutorService.Executor.Types.VmLaunchTypes;

    
internal enum ResourceRequestType
{
    Spawn, Query
}

internal class ResourceRequest
{
    public ResourceRequestType Type { get; set; }
    public FilesystemType RequiredAllocation { get; set; } // for spawns
    public TaskCompletionSource<bool> Decision { get; set; }
    public CancellationToken CancellationToken { get; set; }
}
internal class TotalVmResourceAllocation
{
    internal double VcpuCount { get; set; }
    internal long MemMb { get; set; }
}