namespace AlgoDuck.Modules.Cohort.Queries.Admin.Members.GetCohortMembers;

public sealed class GetCohortMembersResultDto
{
    public Guid CohortId { get; init; }
    public int TotalMembers { get; init; }
    public IReadOnlyList<AdminGetCohortMembersItemDto> Members { get; init; } = Array.Empty<AdminGetCohortMembersItemDto>();
}

public sealed class AdminGetCohortMembersItemDto
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime? JoinedAt { get; init; }
}