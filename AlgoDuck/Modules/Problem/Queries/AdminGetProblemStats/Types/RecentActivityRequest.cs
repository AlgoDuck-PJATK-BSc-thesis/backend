namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;

public class RecentActivityRequest
{
    public required Guid ProblemId {get; set;}
    public required int RecentCount { get; set; }    
}
