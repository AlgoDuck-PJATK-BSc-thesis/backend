using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

public interface ITokenRepository
{
    Task<TokenInfoDto?> GetTokenInfoAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TokenInfoDto>> GetUserTokensAsync(Guid userId, CancellationToken cancellationToken);
}