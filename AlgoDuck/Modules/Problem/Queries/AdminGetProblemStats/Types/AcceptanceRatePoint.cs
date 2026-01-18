namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;

public record AcceptanceRatePoint
{
    public string Date { get; init; } = string.Empty;
    public double Rate { get; init; }
}