namespace AlgoDuck.Modules.Cohort.Queries.GetCohortMembers;

public interface IGetCohortMembersHandler
{
    Task<GetCohortMembersResultDto> HandleAsync(Guid userId, GetCohortMembersRequestDto dto, CancellationToken cancellationToken);
}