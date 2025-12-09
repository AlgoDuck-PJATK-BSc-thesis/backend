namespace AlgoDuck.Modules.Cohort.Queries.GetCohortMembers;

public sealed class GetCohortMembersRequestDto
{
    public Guid CohortId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}