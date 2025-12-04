namespace AlgoDuck.Modules.User.Queries.GetUserActivity;

public sealed class UserActivityDto
{
    public Guid SolutionId { get; init; }
    public Guid ProblemId { get; init; }
    public string ProblemName { get; init; } = string.Empty;
    public string StatusName { get; init; } = string.Empty;
    public long CodeRuntimeSubmitted { get; init; }
    public DateTime SubmittedAt { get; init; }
}