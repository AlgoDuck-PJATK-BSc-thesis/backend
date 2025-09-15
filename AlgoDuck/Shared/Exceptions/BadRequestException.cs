namespace AlgoDuck.Shared.Exceptions;

public class BadRequestException : AppException
{
    public BadRequestException(string message = "Bad request")
        : base(message, 400) { }
}