namespace AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.DeleteCohort;

public interface IDeleteCohortHandler
{
    Task HandleAsync(Guid cohortId, CancellationToken cancellationToken);
}