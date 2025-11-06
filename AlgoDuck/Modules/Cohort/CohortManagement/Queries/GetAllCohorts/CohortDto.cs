namespace AlgoDuck.Modules.Cohort.CohortManagement.Queries.GetAllCohorts;

public sealed class CohortDto
{
    public Guid CohortId { get; set; }
    public string Name { get; set; } = default!;
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUsername { get; set; } = default!;
    public int MemberCount { get; set; }
}