using AlgoDuck.Shared.Exceptions;

namespace AlgoDuck.Modules.Cohort.Shared.Exceptions;

public abstract class CohortException : AppException
{
    public string Code { get; }

    protected CohortException(string code, string message, int statusCode = 400)
        : base(message, statusCode)
    {
        Code = code;
    }

    protected CohortException(string code, string message, int statusCode, Exception? innerException)
        : base(message, statusCode, innerException!)
    {
        Code = code;
    }
}