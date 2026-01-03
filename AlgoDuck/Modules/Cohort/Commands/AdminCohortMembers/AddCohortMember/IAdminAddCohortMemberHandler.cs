namespace AlgoDuck.Modules.Cohort.Commands.AdminCohortMembers.AddCohortMember;

public interface IAdminAddCohortMemberHandler
{
    Task HandleAsync(Guid cohortId, AdminAddCohortMemberDto dto, CancellationToken cancellationToken);
}