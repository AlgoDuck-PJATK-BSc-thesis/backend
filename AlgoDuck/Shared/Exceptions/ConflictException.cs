namespace AlgoDuck.Shared.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message = "A conflict occurred with the current state of the resource.")
        : base(message, 409) { }
}