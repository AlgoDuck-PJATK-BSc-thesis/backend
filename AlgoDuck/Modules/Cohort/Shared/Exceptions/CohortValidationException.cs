namespace AlgoDuck.Modules.Cohort.Shared.Exceptions;

public sealed class CohortValidationException : CohortException
{
    public CohortValidationException(string message)
        : base("cohort_validation_error", message)
    {
    }

    public CohortValidationException(string message, Exception? innerException)
        : base("cohort_validation_error", message, innerException)
    {
    }
}