namespace AlgoDuck.Modules.Cohort.Shared.DTOs;

public sealed class CohortItemDto
{
    public Guid CohortId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public Guid? CreatedByUserId { get; init; }
    public string CreatedByDisplay { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}