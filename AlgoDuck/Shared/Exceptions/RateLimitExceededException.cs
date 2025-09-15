namespace AlgoDuck.Shared.Exceptions;

public class RateLimitExceededException : AppException
{
    public RateLimitExceededException(string message = "You have exceeded the rate limit.")
        : base(message, 429) { }
}