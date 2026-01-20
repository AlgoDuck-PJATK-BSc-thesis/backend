namespace AlgoDuck.Modules.Auth.Shared.Exceptions;

public sealed class EmailVerificationException : AuthException
{
    public EmailVerificationException(string message)
        : base("email_verification_error", message)
    {
    }

    public EmailVerificationException(string message, Exception? innerException)
        : base("email_verification_error", message, innerException)
    {
    }
}