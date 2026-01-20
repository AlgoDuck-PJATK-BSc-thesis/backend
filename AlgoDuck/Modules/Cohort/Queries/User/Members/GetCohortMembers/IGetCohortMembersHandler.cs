namespace AlgoDuck.Modules.Cohort.Queries.User.Members.GetCohortMembers;

public interface IGetCohortMembersHandler
{
    Task<GetCohortMembersResultDto> HandleAsync(Guid userId, GetCohortMembersRequestDto dto, CancellationToken cancellationToken);
}