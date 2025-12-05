using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Exceptions;

namespace AlgoDuck.Modules.Auth.Shared.Services;

public sealed class TwoFactorService
{
    private readonly TwoFactor.ITwoFactorService _inner;

    public TwoFactorService(TwoFactor.ITwoFactorService inner)
    {
        _inner = inner;
    }

    public Task<(string challengeId, DateTimeOffset expiresAt)> SendLoginCodeAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return _inner.SendLoginCodeAsync(user, cancellationToken);
    }

    public async Task<Guid> VerifyLoginCodeAsync(string challengeId, string code, CancellationToken cancellationToken)
    {
        var (ok, userId, error) = await _inner.VerifyLoginCodeAsync(challengeId, code, cancellationToken);
        if (!ok)
        {
            throw new TwoFactorException(error ?? "Invalid two-factor code.");
        }

        if (userId == Guid.Empty)
        {
            throw new TwoFactorException("Invalid user identifier for two-factor challenge.");
        }

        return userId;
    }
}