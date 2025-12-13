using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Mappers;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Mappers;

public sealed class ApiKeyMapperTests
{
    [Fact]
    public void ToApiKeyDto_MapsAllFields()
    {
        var now = DateTimeOffset.UtcNow;
        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "my-key",
            Prefix = "prefix",
            KeyHash = "hash",
            KeySalt = "salt",
            CreatedAt = now,
            ExpiresAt = now.AddDays(10),
            RevokedAt = null
        };

        var dto = ApiKeyMapper.ToApiKeyDto(apiKey);

        Assert.Equal(apiKey.Id, dto.Id);
        Assert.Equal(apiKey.Name, dto.Name);
        Assert.Equal(apiKey.Prefix, dto.Prefix);
        Assert.Equal(apiKey.CreatedAt, dto.CreatedAt);
        Assert.Equal(apiKey.ExpiresAt, dto.ExpiresAt);
        Assert.False(dto.IsRevoked);
    }

    [Fact]
    public void ToApiKeyDto_WhenRevokedAtPresent_SetsIsRevokedTrue()
    {
        var now = DateTimeOffset.UtcNow;
        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "revoked",
            Prefix = "p",
            KeyHash = "h",
            KeySalt = "s",
            CreatedAt = now,
            ExpiresAt = null,
            RevokedAt = now.AddMinutes(-1)
        };

        var dto = ApiKeyMapper.ToApiKeyDto(apiKey);

        Assert.True(dto.IsRevoked);
    }
}