namespace AlgoDuck.Shared.Exceptions;

public class TooManyRequestsException : AppException
{
    public TooManyRequestsException(string message = "Too many requests. Please try again later.")
        : base(message, 429) { }
}