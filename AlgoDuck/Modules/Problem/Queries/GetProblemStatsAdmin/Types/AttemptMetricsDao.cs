namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;

public class AttemptMetricsDao
{
    public required Guid UserId { get; set; }    
    public required bool IsAccepted { get; set; }
}