namespace WebApplication1.Modules.ProblemModule.DTOs;

public class ExecuteResultDto
{
    public string? StdOutput { get; init; } = "";
    public string? StdError { get; init; } = "";
    public string? TestResults { get; init; } = "";
}