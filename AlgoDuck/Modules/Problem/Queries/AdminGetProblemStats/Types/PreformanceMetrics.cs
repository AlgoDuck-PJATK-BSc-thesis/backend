namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;

public class PerformanceMetrics
{
    public required long AverageRuntimeNs { get; set; }
    public required long MedianRuntimeMs { get; set; }
    public required long P95RuntimeMs { get; set; }
    public required long AvgJvmMemoryUsageKb { get; set; }
    public required List<BucketData> RuntimeBuckets { get; set; }
    public required List<BucketData> MemoryBuckets { get; set; }
}

public class BucketData
{
    public required string Range { get; set; }
    public required long Count { get; set; }
}