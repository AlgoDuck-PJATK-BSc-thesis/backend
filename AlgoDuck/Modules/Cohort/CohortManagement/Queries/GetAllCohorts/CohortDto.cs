namespace AlgoDuck.Modules.Cohort.CohortManagement.Queries.GetAllCohorts;

public sealed class CohortDto
{
    public Guid CohortId { get; set; }
    public string Name { get; set; } = default!;
}