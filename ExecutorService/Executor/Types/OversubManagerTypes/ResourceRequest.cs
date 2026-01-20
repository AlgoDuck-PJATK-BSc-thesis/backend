using ExecutorService.Executor.Types.FilesystemPoolerTypes;

namespace ExecutorService.Executor.Types.OversubManagerTypes;

internal class ResourceRequest
{
    public ResourceRequestType Type { get; set; }
    public FilesystemType RequiredAllocation { get; set; } // for spawns
    public TaskCompletionSource<bool> Decision { get; set; }
    public CancellationToken CancellationToken { get; set; }
}