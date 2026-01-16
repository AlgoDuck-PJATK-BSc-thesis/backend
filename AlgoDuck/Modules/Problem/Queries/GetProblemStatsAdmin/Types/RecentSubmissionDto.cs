using AlgoDuck.Models;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;

public class RecentSubmissionDto
{
    public required Guid SubmissionId { get; set; }
    public required Guid UserId { get; set; }
    public required string Username { get; set; }
    public required ExecutionResult Status { get; set; }
    public required long RuntimeNs { get; set; }
    public required DateTime SubmittedAt { get; set; }
}