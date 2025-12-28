namespace AlgoDuck.Modules.Cohort.Shared.Exceptions;

public sealed class CohortValidationException : CohortException
{
    public CohortValidationException(string message, int statusCode = 400)
        : base("cohort_validation_error", message, statusCode)
    {
    }

    public CohortValidationException(string message, int statusCode, Exception? innerException)
        : base("cohort_validation_error", message, statusCode, innerException)
    {
    }
}