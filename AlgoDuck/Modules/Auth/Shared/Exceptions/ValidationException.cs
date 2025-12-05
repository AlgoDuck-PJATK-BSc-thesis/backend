namespace AlgoDuck.Modules.Auth.Shared.Exceptions;

public sealed class ValidationException : AuthException
{
    public ValidationException(string message)
        : base("auth_validation_error", message)
    {
    }

    public ValidationException(string message, Exception? innerException)
        : base("auth_validation_error", message, innerException)
    {
    }
}