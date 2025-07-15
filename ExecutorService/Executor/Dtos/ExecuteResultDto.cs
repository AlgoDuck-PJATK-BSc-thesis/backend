namespace ExecutorService.Executor._ExecutorUtils;

public record ExecuteResultDto(string? StdOutput, string? TestResults)
{
    public string? StdOutput { get; set; } = StdOutput;
    public string? TestResults { get; set; } = TestResults;

    public ExecuteResultDto() : this("", "")
    {
    }
};