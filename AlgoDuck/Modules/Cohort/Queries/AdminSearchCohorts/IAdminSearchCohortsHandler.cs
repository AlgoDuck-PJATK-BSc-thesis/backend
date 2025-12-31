namespace AlgoDuck.Modules.Cohort.Queries.AdminSearchCohorts;

public interface IAdminSearchCohortsHandler
{
    Task<AdminSearchCohortsResultDto> HandleAsync(AdminSearchCohortsDto query, CancellationToken cancellationToken);
}