namespace AlgoDuck.Modules.Cohort.Queries.User.Cohorts.GetCohortById;

public sealed class GetCohortByIdResultDto
{
    public Guid CohortId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public Guid CreatedByUserId { get; init; }
    public bool IsMember { get; init; }
    public string? JoinCode { get; init; }
}