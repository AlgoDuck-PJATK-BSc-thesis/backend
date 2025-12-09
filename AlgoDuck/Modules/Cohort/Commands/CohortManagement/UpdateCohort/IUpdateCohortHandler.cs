namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.UpdateCohort;

public interface IUpdateCohortHandler
{
    Task<UpdateCohortResultDto> HandleAsync(Guid userId, Guid cohortId, UpdateCohortDto dto, CancellationToken cancellationToken);
}