namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;

public record TimeSeriesRequest
{
    public Guid ProblemId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public TimeSeriesGranularity Granularity { get; init; } = TimeSeriesGranularity.Day;
}