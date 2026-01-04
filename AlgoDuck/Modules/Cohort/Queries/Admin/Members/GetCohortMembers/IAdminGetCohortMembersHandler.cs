namespace AlgoDuck.Modules.Cohort.Queries.Admin.Members.GetCohortMembers;

public interface IAdminGetCohortMembersHandler
{
    Task<GetCohortMembersResultDto> HandleAsync(GetCohortMembersRequestDto dto, CancellationToken cancellationToken);
}