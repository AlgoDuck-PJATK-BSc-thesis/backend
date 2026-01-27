using AlgoDuck.Modules.Cohort.Shared.DTOs;
using AlgoDuck.Shared.Types;

namespace AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.SearchCohorts;

public sealed class SearchCohortsResultDto
{
    public CohortItemDto? IdMatch { get; init; }
    public PageData<CohortItemDto> Name { get; init; } = new();
}
