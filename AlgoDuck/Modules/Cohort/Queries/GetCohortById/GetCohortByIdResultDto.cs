namespace AlgoDuck.Modules.Cohort.Queries.GetCohortById;

public sealed class GetCohortByIdResultDto
{
    public Guid CohortId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public Guid CreatedByUserId { get; init; }
    public bool IsMember { get; init; }
}