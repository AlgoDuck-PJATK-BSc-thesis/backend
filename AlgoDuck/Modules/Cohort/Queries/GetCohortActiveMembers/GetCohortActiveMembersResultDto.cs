namespace AlgoDuck.Modules.Cohort.Queries.GetCohortActiveMembers;

public sealed class GetCohortActiveMembersResultDto
{
    public IReadOnlyList<GetCohortActiveMembersItemDto> Items { get; init; } = Array.Empty<GetCohortActiveMembersItemDto>();
}

public sealed class GetCohortActiveMembersItemDto
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? UserAvatarUrl { get; init; }
    public DateTimeOffset LastSeenAt { get; init; }
    public bool IsActive { get; init; }
}