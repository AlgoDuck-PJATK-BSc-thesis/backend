namespace AlgoDuck.Modules.Cohort.Commands.Admin.Members.RemoveCohortMember;

public interface IRemoveCohortMemberHandler
{
    Task HandleAsync(Guid cohortId, Guid userId, CancellationToken cancellationToken);
}