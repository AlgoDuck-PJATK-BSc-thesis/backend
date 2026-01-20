namespace ExecutorService.Executor.Types.VmLaunchTypes;

public class VmTerminationEvent
{
    public Guid VmId { get; init; }
    public VmTerminationReason Reason { get; init; }
    public DateTime TerminatedAt { get; init; } = DateTime.UtcNow;
    public List<Guid> AffectedJobs { get; init; } = [];
    public TimeSpan Lifetime { get; init; }
}