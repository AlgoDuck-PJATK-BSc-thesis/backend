namespace ExecutorService.Executor.Dtos;

public class ExecuteResultDto
{
    public string StdOutput { get; set; } = string.Empty;
    public string StdError { get; set; } = string.Empty;
    public List<TestResultDto> TestResults { get; set; } = [];
    public int ExecutionTime { get; set; } = 0;
}