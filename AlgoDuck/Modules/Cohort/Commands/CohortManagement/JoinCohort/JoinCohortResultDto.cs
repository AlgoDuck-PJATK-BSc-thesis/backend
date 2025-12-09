namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.JoinCohort;

public sealed class JoinCohortResultDto
{
    public Guid CohortId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid CreatedByUserId { get; init; }
    public bool IsActive { get; init; }
}