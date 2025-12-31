namespace AlgoDuck.Modules.Cohort.Queries.AdminGetCohorts;

public sealed class AdminGetCohortsDto
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}