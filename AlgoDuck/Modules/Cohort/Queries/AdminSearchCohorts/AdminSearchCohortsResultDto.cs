using AlgoDuck.Modules.Cohort.Queries.AdminShared;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Cohort.Queries.AdminSearchCohorts;

public sealed class AdminSearchCohortsResultDto
{
    public AdminCohortItemDto? IdMatch { get; init; }
    public PageData<AdminCohortItemDto> Name { get; init; } = new();
}