using AlgoDuck.Modules.Cohort.Shared.Dtos;

namespace AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.UpdateCohort;

public interface IUpdateCohortHandler
{
    Task<CohortItemDto> HandleAsync(Guid cohortId, UpdateCohortDto dto, CancellationToken cancellationToken);
}