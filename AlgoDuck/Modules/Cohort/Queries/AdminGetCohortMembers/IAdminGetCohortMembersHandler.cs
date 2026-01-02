namespace AlgoDuck.Modules.Cohort.Queries.AdminGetCohortMembers;

public interface IAdminGetCohortMembersHandler
{
    Task<AdminGetCohortMembersResultDto> HandleAsync(AdminGetCohortMembersRequestDto dto, CancellationToken cancellationToken);
}