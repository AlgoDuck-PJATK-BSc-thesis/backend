namespace AlgoDuck.Modules.Auth.Shared.Exceptions;

public sealed class TwoFactorException : AuthException
{
    public TwoFactorException(string message)
        : base("two_factor_error", message)
    {
    }

    public TwoFactorException(string message, Exception? innerException)
        : base("two_factor_error", message, innerException)
    {
    }
}