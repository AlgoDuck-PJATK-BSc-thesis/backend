namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.LeaveCohort;

public interface ILeaveCohortHandler
{
    Task HandleAsync(Guid userId, CancellationToken cancellationToken);
}