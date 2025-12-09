namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.CreateCohort;

public sealed class CreateCohortResultDto
{
    public Guid CohortId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid CreatedByUserId { get; init; }
    public bool IsActive { get; init; }
}