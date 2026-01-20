namespace AlgoDuck.Modules.Cohort.Commands.User.Management.UpdateCohort;

public interface IUpdateCohortHandler
{
    Task<UpdateCohortResultDto> HandleAsync(Guid userId, Guid cohortId, UpdateCohortDto dto, CancellationToken cancellationToken);
}