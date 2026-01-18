namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;

public class PerformanceRequest
{
    public required Guid ProblemId {get; set;}
    public required int RuntimeBucketSize { get; set; }
    public required int MemoryBucketSize { get; set; }
}

