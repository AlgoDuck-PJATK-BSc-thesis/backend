namespace AlgoDuck.Modules.Cohort.Commands.User.Management.LeaveCohort;

public interface ILeaveCohortHandler
{
    Task HandleAsync(Guid userId, CancellationToken cancellationToken);
}