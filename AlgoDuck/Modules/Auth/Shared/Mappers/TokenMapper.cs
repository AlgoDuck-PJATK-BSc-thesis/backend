using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Shared.Mappers;

public static class TokenMapper
{
    public static TokenInfoDto ToTokenInfoDto(RefreshToken token)
    {
        return new TokenInfoDto
        {
            Id = token.Id,
            UserId = token.UserId,
            SessionId = token.SessionId,
            ExpiresAt = token.ExpiresAt,
            IsRevoked = token.RevokedAt.HasValue
        };
    }
}