namespace AlgoDuck.Modules.Cohort.Commands.User.Management.UpdateCohort;

public sealed class UpdateCohortResultDto
{
    public Guid CohortId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid CreatedByUserId { get; init; }
    public bool IsActive { get; init; }
}