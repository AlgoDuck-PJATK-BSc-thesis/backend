namespace ExecutorService.Executor.Dtos;

public class ExecuteResultDto
{
    public string StdOutput { get; set; } = string.Empty;
    public string StdError { get; set; } = string.Empty;
    public string TestResults { get; set; } = string.Empty;
}