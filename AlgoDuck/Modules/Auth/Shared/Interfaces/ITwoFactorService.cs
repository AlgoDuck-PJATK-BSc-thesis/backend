using AlgoDuck.Models;

namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

public interface ITwoFactorService
{
    Task<(string challengeId, DateTimeOffset expiresAt)> SendLoginCodeAsync(ApplicationUser user, CancellationToken cancellationToken);
    Task<(bool ok, Guid userId, string? error)> VerifyLoginCodeAsync(string challengeId, string code, CancellationToken cancellationToken);
}