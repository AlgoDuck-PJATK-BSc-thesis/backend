namespace AlgoDuck.Modules.Cohort.Queries.User.Cohorts.GetUserCohorts;

public interface IGetUserCohortsHandler
{
    Task<GetUserCohortsResultDto> HandleAsync(Guid userId, CancellationToken cancellationToken);
}