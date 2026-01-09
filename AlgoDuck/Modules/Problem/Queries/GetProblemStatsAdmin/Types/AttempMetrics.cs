namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;

public class AttemptMetrics
{
    public required int TotalAttempts { get; init; }
    public required int UniqueAttempts { get; init; }
    public required int AcceptedAttempts { get; init; }
    public required int AcceptedUniqueAttempts { get; init; }
    public required double AcceptanceRate { get; init; }
    
}