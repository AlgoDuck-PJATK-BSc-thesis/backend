namespace AlgoDuck.Modules.Cohort.Commands.User.Management.CreateCohort;

public interface ICreateCohortHandler
{
    Task<CreateCohortResultDto> HandleAsync(Guid userId, CreateCohortDto dto, CancellationToken cancellationToken);
}