namespace ExecutorService.Executor.Dtos;

public class ExecuteResultDto
{
    public string? StdOutput { get; set; } = "";
    public string? StdError { get; set; } = "";
    public string? TestResults { get; set; } = "";
}