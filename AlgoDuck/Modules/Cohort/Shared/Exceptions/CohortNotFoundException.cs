namespace AlgoDuck.Modules.Cohort.Shared.Exceptions;

public sealed class CohortNotFoundException : CohortException
{
    public Guid? CohortId { get; }

    public CohortNotFoundException(Guid cohortId)
        : base("cohort_not_found", $"Cohort '{cohortId}' not found.", 404)
    {
        CohortId = cohortId;
    }

    public CohortNotFoundException(string message = "Cohort not found.")
        : base("cohort_not_found", message, 404)
    {
    }

    public CohortNotFoundException(string message, Exception? innerException)
        : base("cohort_not_found", message, 404, innerException)
    {
    }
}