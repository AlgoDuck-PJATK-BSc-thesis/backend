namespace AlgoDuck.Shared.Exceptions;

public class UsernameAlreadyExistsException : AppException
{
    public UsernameAlreadyExistsException(string message = "This username is already taken.")
        : base(message, 409) { }
}