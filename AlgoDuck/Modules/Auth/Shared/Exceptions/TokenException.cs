namespace AlgoDuck.Modules.Auth.Shared.Exceptions;

public sealed class TokenException : AuthException
{
    public TokenException(string message)
        : base("token_error", message)
    {
    }

    public TokenException(string message, Exception? innerException)
        : base("token_error", message, innerException)
    {
    }
}