namespace AlgoDuck.Modules.Cohort.Commands.User.Management.JoinCohort;

public interface IJoinCohortHandler
{
    Task<JoinCohortResultDto> HandleAsync(Guid userId, Guid cohortId, CancellationToken cancellationToken);
}