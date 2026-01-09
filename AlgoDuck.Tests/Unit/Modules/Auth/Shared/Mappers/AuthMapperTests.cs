using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Mappers;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Mappers;

public sealed class AuthMapperTests
{
    [Fact]
    public void ToAuthResponse_MapsTokensDatesAndIds()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            Email = "alice@example.com",
            EmailConfirmed = true
        };

        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = user.Id,
            RefreshTokenHash = "h",
            RefreshTokenSalt = "s",
            RefreshTokenPrefix = "p",
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
        };

        var accessToken = "access";
        var refreshToken = "refresh";
        var csrfToken = "csrf";
        var accessExpires = DateTimeOffset.UtcNow.AddMinutes(5);
        var refreshExpires = DateTimeOffset.UtcNow.AddMinutes(60);

        var dto = AuthMapper.ToAuthResponse(
            user,
            session,
            accessToken,
            refreshToken,
            csrfToken,
            accessExpires,
            refreshExpires);

        Assert.Equal(accessToken, dto.AccessToken);
        Assert.Equal(refreshToken, dto.RefreshToken);
        Assert.Equal(csrfToken, dto.CsrfToken);
        Assert.Equal(accessExpires, dto.AccessTokenExpiresAt);
        Assert.Equal(refreshExpires, dto.RefreshTokenExpiresAt);
        Assert.Equal(session.SessionId, dto.SessionId);
        Assert.Equal(user.Id, dto.UserId);
    }

    [Fact]
    public void ToRefreshResult_MapsTokensDatesAndIds_FromSession()
    {
        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RefreshTokenHash = "h",
            RefreshTokenSalt = "s",
            RefreshTokenPrefix = "p",
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
        };

        var accessToken = "access";
        var refreshToken = "refresh";
        var csrfToken = "csrf";
        var accessExpires = DateTimeOffset.UtcNow.AddMinutes(5);
        var refreshExpires = DateTimeOffset.UtcNow.AddMinutes(60);

        var dto = AuthMapper.ToRefreshResult(
            session,
            accessToken,
            refreshToken,
            csrfToken,
            accessExpires,
            refreshExpires);

        Assert.Equal(accessToken, dto.AccessToken);
        Assert.Equal(refreshToken, dto.RefreshToken);
        Assert.Equal(csrfToken, dto.CsrfToken);
        Assert.Equal(accessExpires, dto.AccessTokenExpiresAt);
        Assert.Equal(refreshExpires, dto.RefreshTokenExpiresAt);
        Assert.Equal(session.SessionId, dto.SessionId);
        Assert.Equal(session.UserId, dto.UserId);
    }
}
