using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

public interface ITokenService
{
    Task<AuthResponse> GenerateAuthTokensAsync(ApplicationUser user, CancellationToken cancellationToken);
    Task<RefreshResult> RefreshTokensAsync(Session session, CancellationToken cancellationToken);
    Task<TokenInfoDto> GetTokenInfoAsync(Guid sessionId, CancellationToken cancellationToken);
}