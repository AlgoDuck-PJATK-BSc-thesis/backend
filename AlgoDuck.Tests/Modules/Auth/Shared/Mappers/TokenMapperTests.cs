using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Mappers;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Mappers;

public sealed class TokenMapperTests
{
    [Fact]
    public void ToTokenInfoDto_MapsAllFields()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(10);
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            TokenHash = "h",
            TokenSalt = "s",
            TokenPrefix = "p",
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = expiresAt,
            RevokedAt = null,
            ReplacedByTokenId = null
        };

        var dto = TokenMapper.ToTokenInfoDto(token);

        Assert.Equal(token.Id, dto.Id);
        Assert.Equal(token.UserId, dto.UserId);
        Assert.Equal(token.SessionId, dto.SessionId);
        Assert.Equal(token.ExpiresAt, dto.ExpiresAt);
        Assert.False(dto.IsRevoked);
    }

    [Fact]
    public void ToTokenInfoDto_WhenRevokedAtPresent_SetsIsRevokedTrue()
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            TokenHash = "h",
            TokenSalt = "s",
            TokenPrefix = "p",
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            RevokedAt = DateTimeOffset.UtcNow.AddMinutes(-1)
        };

        var dto = TokenMapper.ToTokenInfoDto(token);

        Assert.True(dto.IsRevoked);
    }
}