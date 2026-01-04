using AlgoDuck.Modules.Cohort.Shared.Dtos;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.GetCohorts;


public interface IAdminGetCohortsHandler
{
    Task<PageData<CohortItemDto>> HandleAsync(AdminGetCohortsDto query, CancellationToken cancellationToken);
}