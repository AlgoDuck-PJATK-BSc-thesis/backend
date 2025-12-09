namespace AlgoDuck.Modules.Cohort.Shared.Exceptions;

public abstract class CohortException : Exception
{
    public string Code { get; }

    protected CohortException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    protected CohortException(string code, string message, Exception? innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}