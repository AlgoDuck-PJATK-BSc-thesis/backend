namespace ExecutorService.Executor.Types.VmLaunchTypes;

internal class VmResourceAllocation
{
    internal int VcpuCount { get; set; }
    internal int MemMB { get; set; }
    internal bool Smt { get; set; } = false;
}