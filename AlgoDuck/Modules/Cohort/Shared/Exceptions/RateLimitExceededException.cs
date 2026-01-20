namespace AlgoDuck.Modules.Cohort.Shared.Exceptions;

public sealed class RateLimitExceededException : Exception
{
    public RateLimitExceededException(string message) : base(message)
    {
    }
}