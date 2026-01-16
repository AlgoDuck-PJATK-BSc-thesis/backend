namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;

public record TimeSeriesMetrics
{
    public ICollection<AcceptanceRatePoint> AcceptanceRateHistory { get; init; } = [];
    public ICollection<SubmissionPoint> SubmissionsOverTime { get; init; } = [];
}