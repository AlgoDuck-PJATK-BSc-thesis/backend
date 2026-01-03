namespace AlgoDuck.Modules.Cohort.Commands.AdminCohortMembers.RemoveCohortMember;

public interface IAdminRemoveCohortMemberHandler
{
    Task HandleAsync(Guid cohortId, Guid userId, CancellationToken cancellationToken);
}