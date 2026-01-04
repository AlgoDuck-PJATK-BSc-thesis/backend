namespace AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.SearchCohorts;

public interface ISearchCohortsHandler
{
    Task<SearchCohortsResultDto> HandleAsync(SearchCohortsDto query, CancellationToken cancellationToken);
}