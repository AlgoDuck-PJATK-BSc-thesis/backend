using AlgoDuck.Modules.Cohort.Queries.AdminShared;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Cohort.Queries.AdminGetCohorts;

public interface IAdminGetCohortsHandler
{
    Task<PageData<AdminCohortItemDto>> HandleAsync(AdminGetCohortsDto query, CancellationToken cancellationToken);
}