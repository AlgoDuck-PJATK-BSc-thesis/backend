namespace ExecutorService.Executor.Types.VmLaunchTypes;

internal class VmIoData
{
    internal long BytesRead { get; set; }
    internal long BytesWritten { get; set; }
}
    
internal class VmResourceUsage
{
    internal double CpuTime { get; set; }
    internal long WorkingMemBytes { get; set; }
    internal VmIoData IoData { get; } = new(); // useful for inspecting compiler upon detecting suspiciously large io spikes
    internal TimeSpan LastCpuTime = TimeSpan.Zero;
}