using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Commands.RefreshToken;

public interface IRefreshTokenHandler
{
    Task<RefreshResult> HandleAsync(RefreshTokenDto dto, CancellationToken cancellationToken);
}