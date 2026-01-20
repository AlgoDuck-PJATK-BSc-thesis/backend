using System.Diagnostics;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.Types.FilesystemPoolerTypes;

namespace ExecutorService.Executor.Types.VmLaunchTypes;

public class VmConfig
{
    internal Guid VmId { get; set; }
    internal string? VmName { get; set; }
    internal FilesystemType VmType { get; set; }
    internal VmResourceAllocation? AllocatedResources { get; set; }
    internal VmResourceUsage UsedResources { get; } = new();
    internal Guid FilesystemId { get; set; }
    internal int GuestCid { get; set; }
    internal string? VsockPath { get; set; }
    internal int Pid { get; set; }
    internal Process? VmProcess { get; set; }
    internal List<Guid> ServicedJobs { get; set; } = [];
    internal Dictionary<string, string> FileHashes { get; set; } = [];
    
    internal DateTime LaunchedAt { get; set; } = DateTime.UtcNow;
    internal CancellationTokenSource? LifetimeCts { get; set; }
    internal TaskCompletionSource<VmTerminationReason> TerminationTcs { get; set; } = new();
}

public enum VmTerminationReason
{
    Normal,
    Timeout,
    ReaperKill,
    WatchdogKill,
    ProcessCrash
}