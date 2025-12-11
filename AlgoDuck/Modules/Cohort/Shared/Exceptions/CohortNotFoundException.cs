namespace AlgoDuck.Modules.Cohort.Shared.Exceptions;

public sealed class CohortNotFoundException : CohortException
{
    public CohortNotFoundException(Guid cohortId)
        : base("cohort_not_found", $"Cohort with id '{cohortId}' was not found.")
    {
    }

    public CohortNotFoundException(string message)
        : base("cohort_not_found", message)
    {
    }

    public CohortNotFoundException(string message, Exception? innerException)
        : base("cohort_not_found", message, innerException)
    {
    }
}