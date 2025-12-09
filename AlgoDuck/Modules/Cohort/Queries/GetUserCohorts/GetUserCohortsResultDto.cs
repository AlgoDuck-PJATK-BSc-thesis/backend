namespace AlgoDuck.Modules.Cohort.Queries.GetUserCohorts;

public sealed class GetUserCohortsResultDto
{
    public IReadOnlyList<GetUserCohortsItemDto> Items { get; init; } = Array.Empty<GetUserCohortsItemDto>();
}

public sealed class GetUserCohortsItemDto
{
    public Guid CohortId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public Guid CreatedByUserId { get; init; }
}