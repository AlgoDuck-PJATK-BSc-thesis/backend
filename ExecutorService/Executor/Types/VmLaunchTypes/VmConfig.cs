using System.Diagnostics;
using ExecutorService.Executor.ResourceHandlers;

namespace ExecutorService.Executor.Types.VmLaunchTypes;

internal class VmConfig
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
    internal int ServicedRequests { get; set; }
    internal Dictionary<string, string> FileHashes { get; set; } = [];
}