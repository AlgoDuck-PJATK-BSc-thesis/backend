using AlgoDuck.Modules.Cohort.Shared.Dtos;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.SearchCohorts;

public sealed class SearchCohortsResultDto
{
    public CohortItemDto? IdMatch { get; init; }
    public PageData<CohortItemDto> Name { get; init; } = new();
}
