namespace AlgoDuck.Modules.Cohort.Queries.GetCohortMembers;

public sealed class GetCohortMembersResultDto
{
    public IReadOnlyList<GetCohortMembersItemDto> Items { get; init; } = Array.Empty<GetCohortMembersItemDto>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}

public sealed class GetCohortMembersItemDto
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? UserAvatarUrl { get; init; }
    public bool IsYou { get; init; }
}