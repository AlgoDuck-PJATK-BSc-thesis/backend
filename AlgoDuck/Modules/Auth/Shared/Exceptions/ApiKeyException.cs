namespace AlgoDuck.Modules.Auth.Shared.Exceptions;

public sealed class ApiKeyException : AuthException
{
    public ApiKeyException(string message)
        : base("api_key_error", message)
    {
    }

    public ApiKeyException(string message, Exception? innerException)
        : base("api_key_error", message, innerException)
    {
    }
}