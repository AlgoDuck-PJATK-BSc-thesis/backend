using AlgoDuck.Modules.Cohort.Shared.DTOs;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.Types;

namespace AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.GetCohorts;


public interface IAdminGetCohortsHandler
{
    Task<PageData<CohortItemDto>> HandleAsync(AdminGetCohortsDto query, CancellationToken cancellationToken);
}