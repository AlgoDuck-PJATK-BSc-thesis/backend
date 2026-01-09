using AlgoDuckShared;

namespace AlgoDuck.Modules.Problem.Shared.Types;

public class SubmitExecuteResponse
{
    public string StdOutput { get; set; } = string.Empty;
    public string StdError { get; set; } = string.Empty;
    public List<TestResultDto> TestResults { get; set; } = [];
    public long ExecutionStartTimeNs { get; set; }
    public long ExecutionEndTimeNs { get; set; }
    public int ExecutionExitCode { get; set; }
    public long ExecutionTimeNs => ExecutionEndTimeNs - ExecutionStartTimeNs;
    public long JvmMemoryPeakKb  { get; set; }
    public SubmitExecuteRequestRabbitStatus Status { get; set; }
}
