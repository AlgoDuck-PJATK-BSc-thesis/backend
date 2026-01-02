namespace AlgoDuck.Modules.Cohort.Queries.AdminShared;

public sealed class AdminCohortItemDto
{
    public Guid CohortId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public Guid? CreatedByUserId { get; init; }
    public string CreatedByDisplay { get; init; } = string.Empty;
}