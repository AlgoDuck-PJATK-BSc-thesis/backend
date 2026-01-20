namespace AlgoDuck.Modules.Auth.Shared.Validators;

public sealed class TokenValidator : BaseValidator
{
    public void EnsureNotExpired(DateTimeOffset expiresAt)
    {
        Ensure(expiresAt > DateTimeOffset.UtcNow, "Token has expired.");
    }

    public void EnsureNotRevoked(bool isRevoked)
    {
        Ensure(!isRevoked, "Token has been revoked.");
    }
}