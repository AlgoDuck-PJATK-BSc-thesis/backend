using AlgoDuck.Models;

namespace AlgoDuck.Modules.Auth.TwoFactor
{
    public interface ITwoFactorService
    {
        Task<(string challengeId, DateTimeOffset expiresAt)> SendLoginCodeAsync(ApplicationUser user, CancellationToken ct);
        Task<(bool ok, Guid userId, string? error)> VerifyLoginCodeAsync(string challengeId, string code, CancellationToken ct);
    }
}