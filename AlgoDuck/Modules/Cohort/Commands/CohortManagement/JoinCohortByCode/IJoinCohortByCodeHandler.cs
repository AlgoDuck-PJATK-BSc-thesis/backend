using AlgoDuck.Modules.Cohort.Commands.CohortManagement.JoinCohort;

namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.JoinCohortByCode;

public interface IJoinCohortByCodeHandler
{
    Task<JoinCohortResultDto> HandleAsync(Guid userId, JoinCohortByCodeDto dto, CancellationToken cancellationToken);
}