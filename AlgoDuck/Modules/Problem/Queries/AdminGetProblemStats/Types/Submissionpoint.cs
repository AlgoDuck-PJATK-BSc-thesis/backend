namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;

public record SubmissionPoint
{
    public string Date { get; init; } = string.Empty;
    public int Count { get; init; }
    public int Passed { get; init; }
}