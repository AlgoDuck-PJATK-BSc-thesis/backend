using AlgoDuck.Modules.Cohort.Shared.Dtos;

namespace AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.CreateCohort;

public interface ICreateCohortHandler
{
    Task<CohortItemDto> HandleAsync(Guid adminUserId, CreateCohortDto dto, CancellationToken cancellationToken);
}