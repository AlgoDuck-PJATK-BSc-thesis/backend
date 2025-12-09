namespace AlgoDuck.Modules.Cohort.Queries.GetCohortActiveMembers;

public interface IGetCohortActiveMembersHandler
{
    Task<GetCohortActiveMembersResultDto> HandleAsync(
        Guid userId,
        GetCohortActiveMembersRequestDto dto,
        CancellationToken cancellationToken);
}