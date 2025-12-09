namespace AlgoDuck.Modules.Cohort.Shared.Utils;

public sealed class CohortActiveUser
{
    public Guid UserId { get; init; }
    public DateTimeOffset LastSeenAt { get; init; }
}