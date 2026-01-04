namespace AlgoDuck.Modules.Cohort.Queries.User.Members.GetCohortActiveMembers;

public interface IGetCohortActiveMembersHandler
{
    Task<GetCohortActiveMembersResultDto> HandleAsync(
        Guid userId,
        GetCohortActiveMembersRequestDto dto,
        CancellationToken cancellationToken);
}