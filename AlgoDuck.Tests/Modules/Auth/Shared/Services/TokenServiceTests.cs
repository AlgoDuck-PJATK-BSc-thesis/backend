using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using AlgoDuck.Modules.Auth.Shared.Services;
using AlgoDuck.Tests.TestInfrastructure;
using Microsoft.Extensions.Options;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Services;

public class TokenServiceTests : AuthTestBase
{
    [Fact]
    public async Task GenerateAuthTokensAsync_WhenUserIsValid_ThenReturnsAuthResponseWithTokensAndSessionMetadata()
    {
        var sessionRepositoryMock = new Mock<ISessionRepository>();
        var tokenRepositoryMock = new Mock<ITokenRepository>();
        var commandDbContext = CreateCommandDbContext();
        var jwtSettings = CreateJwtSettings();
        var jwtOptions = Options.Create(jwtSettings);
        var jwtTokenProvider = new JwtTokenProvider(jwtOptions);
        var user = CreateUser();
        Session capturedSession = null;

        sessionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .Callback<Session, CancellationToken>((s, _) => capturedSession = s)
            .Returns(Task.CompletedTask);

        sessionRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new TokenService(
            sessionRepositoryMock.Object,
            tokenRepositoryMock.Object,
            commandDbContext,
            jwtTokenProvider,
            jwtOptions);

        var result = await service.GenerateAuthTokensAsync(user, CreateCancellationToken());

        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.False(string.IsNullOrWhiteSpace(result.CsrfToken));
        Assert.Equal(user.Id, result.UserId);
        Assert.NotEqual(Guid.Empty, result.SessionId);
        Assert.True(result.RefreshTokenExpiresAt > result.AccessTokenExpiresAt);

        Assert.NotNull(capturedSession);
        Assert.Equal(user.Id, capturedSession.UserId);
        Assert.False(string.IsNullOrWhiteSpace(capturedSession.RefreshTokenHash));
        Assert.False(string.IsNullOrWhiteSpace(capturedSession.RefreshTokenSalt));
        Assert.False(string.IsNullOrWhiteSpace(capturedSession.RefreshTokenPrefix));
        Assert.True(capturedSession.ExpiresAtUtc > capturedSession.CreatedAtUtc);
        Assert.Equal(result.SessionId, capturedSession.SessionId);

        sessionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Once);
        sessionRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTokenInfoAsync_WhenSessionExists_ThenReturnsTokenInfo()
    {
        var sessionRepositoryMock = new Mock<ISessionRepository>();
        var tokenRepositoryMock = new Mock<ITokenRepository>();
        var commandDbContext = CreateCommandDbContext();
        var jwtSettings = CreateJwtSettings();
        var jwtOptions = Options.Create(jwtSettings);
        var jwtTokenProvider = new JwtTokenProvider(jwtOptions);
        var sessionId = Guid.NewGuid();

        var tokenInfo = new TokenInfoDto
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SessionId = sessionId,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            IsRevoked = false
        };

        tokenRepositoryMock
            .Setup(x => x.GetTokenInfoAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenInfo);

        var service = new TokenService(
            sessionRepositoryMock.Object,
            tokenRepositoryMock.Object,
            commandDbContext,
            jwtTokenProvider,
            jwtOptions);

        var result = await service.GetTokenInfoAsync(sessionId, CreateCancellationToken());

        Assert.Equal(tokenInfo.Id, result.Id);
        Assert.Equal(tokenInfo.UserId, result.UserId);
        Assert.Equal(tokenInfo.SessionId, result.SessionId);
        Assert.Equal(tokenInfo.ExpiresAt, result.ExpiresAt);
        Assert.Equal(tokenInfo.IsRevoked, result.IsRevoked);

        tokenRepositoryMock.Verify(x => x.GetTokenInfoAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTokenInfoAsync_WhenSessionDoesNotExist_ThenThrowsTokenException()
    {
        var sessionRepositoryMock = new Mock<ISessionRepository>();
        var tokenRepositoryMock = new Mock<ITokenRepository>();
        var commandDbContext = CreateCommandDbContext();
        var jwtSettings = CreateJwtSettings();
        var jwtOptions = Options.Create(jwtSettings);
        var jwtTokenProvider = new JwtTokenProvider(jwtOptions);
        var sessionId = Guid.NewGuid();

        tokenRepositoryMock
            .Setup(x => x.GetTokenInfoAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenInfoDto)null);

        var service = new TokenService(
            sessionRepositoryMock.Object,
            tokenRepositoryMock.Object,
            commandDbContext,
            jwtTokenProvider,
            jwtOptions);

        await Assert.ThrowsAsync<TokenException>(() => service.GetTokenInfoAsync(sessionId, CreateCancellationToken()));

        tokenRepositoryMock.Verify(x => x.GetTokenInfoAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
