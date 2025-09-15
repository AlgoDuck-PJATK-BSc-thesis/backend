namespace AlgoDuck.Shared.Exceptions;

public class PayloadTooLargeException : AppException
{
    public PayloadTooLargeException(string message = "The request payload is too large.")
        : base(message, 413) { }
}