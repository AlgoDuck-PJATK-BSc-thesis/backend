namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.JoinCohort;

public interface IJoinCohortHandler
{
    Task<JoinCohortResultDto> HandleAsync(Guid userId, Guid cohortId, CancellationToken cancellationToken);
}