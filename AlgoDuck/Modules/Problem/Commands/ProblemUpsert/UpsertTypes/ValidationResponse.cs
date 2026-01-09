namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;

public class ValidationResponse
{
    public ValidationResponseStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
}