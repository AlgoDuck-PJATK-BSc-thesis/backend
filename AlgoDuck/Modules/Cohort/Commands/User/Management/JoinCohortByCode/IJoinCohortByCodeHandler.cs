using AlgoDuck.Modules.Cohort.Commands.User.Management.JoinCohort;

namespace AlgoDuck.Modules.Cohort.Commands.User.Management.JoinCohortByCode;

public interface IJoinCohortByCodeHandler
{
    Task<JoinCohortResultDto> HandleAsync(Guid userId, JoinCohortByCodeDto dto, CancellationToken cancellationToken);
}