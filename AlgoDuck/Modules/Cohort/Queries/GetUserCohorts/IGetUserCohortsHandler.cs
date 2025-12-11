namespace AlgoDuck.Modules.Cohort.Queries.GetUserCohorts;

public interface IGetUserCohortsHandler
{
    Task<GetUserCohortsResultDto> HandleAsync(Guid userId, CancellationToken cancellationToken);
}