namespace AlgoDuck.Modules.Cohort.Commands.Admin.Members.AddCohortMember;

public interface IAddCohortMemberHandler
{
    Task HandleAsync(Guid cohortId, AddCohortMemberDto dto, CancellationToken cancellationToken);
}