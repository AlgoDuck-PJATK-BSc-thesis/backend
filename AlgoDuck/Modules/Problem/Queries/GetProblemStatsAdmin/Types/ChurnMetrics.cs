namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;

public class ChurnMetrics
{
    public required int UserStartCount{  get; init; }
    public required int UserSubmitCount { get; init; }
    public required int UserFinishCount { get; init; }
}