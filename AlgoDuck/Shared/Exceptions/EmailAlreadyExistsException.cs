namespace AlgoDuck.Shared.Exceptions;

public class EmailAlreadyExistsException : AppException
{
    public EmailAlreadyExistsException(string message = "A user with this email already exists.")
        : base(message, 409) { }
}