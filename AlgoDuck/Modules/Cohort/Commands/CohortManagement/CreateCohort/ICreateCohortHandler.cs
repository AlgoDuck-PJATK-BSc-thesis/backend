namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.CreateCohort;

public interface ICreateCohortHandler
{
    Task<CreateCohortResultDto> HandleAsync(Guid userId, CreateCohortDto dto, CancellationToken cancellationToken);
}