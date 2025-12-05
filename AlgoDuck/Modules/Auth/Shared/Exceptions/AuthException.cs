namespace AlgoDuck.Modules.Auth.Shared.Exceptions;

public abstract class AuthException : Exception
{
    public string Code { get; }

    protected AuthException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    protected AuthException(string code, string message, Exception? innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}