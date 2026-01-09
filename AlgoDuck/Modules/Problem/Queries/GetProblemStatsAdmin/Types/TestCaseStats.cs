namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;
public class TestCaseStats
{
    public required Guid TestCaseId {get; set;}
    public required bool IsPublic {get; set;}
    public required double PassRate { get; set; }
}