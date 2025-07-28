namespace ExecutorService.Executor.Dtos;

public record ExecuteResultDto(string? StdOutput, string? StdError, string? TestResults)
{
    public string? StdOutput { get; set; } = StdOutput;
    public string? StdError { get; set; } = StdOutput;
    public string? TestResults { get; set; } = TestResults;

    public ExecuteResultDto() : this("", "", "")
    {
    }
};