using AlgoDuck.Modules.Cohort.Shared.DTOs;

namespace AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.CreateCohort;

public interface ICreateCohortHandler
{
    Task<CohortItemDto> HandleAsync(Guid adminUserId, CreateCohortDto dto, CancellationToken cancellationToken);
}