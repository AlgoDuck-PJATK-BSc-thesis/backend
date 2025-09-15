namespace AlgoDuck.Modules.Cohort.DTOs;

public class CohortDto
{
    public Guid CohortId { get; set; }
    public string Name { get; set; } = default!;
    public string ImageUrl { get; set; } = default!;
}