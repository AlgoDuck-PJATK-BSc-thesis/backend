namespace AlgoDuck.Modules.User.Queries.User.Activity.GetUserActivity;

public sealed class UserActivityDto
{
    public Guid SolutionId { get; init; }
    public Guid ProblemId { get; init; }
    public string ProblemName { get; init; } = string.Empty;
    public long CodeRuntimeSubmitted { get; init; }
    public DateTime SubmittedAt { get; init; }
}