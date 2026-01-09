using System.Security.Claims;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using AlgoDuck.Modules.Auth.Shared.Services;
using AlgoDuck.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Services;

public class TokenServiceTests : AuthTestBase
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            Options.Create(new IdentityOptions()),
            new Mock<IPasswordHasher<ApplicationUser>>().Object,
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new Mock<ILookupNormalizer>().Object,
            new IdentityErrorDescriber(),
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object
        );

        mgr.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>());

        return mgr;
    }

    [Fact]
    public async Task GenerateAuthTokensAsync_WhenUserIsValid_ThenReturnsAuthResponseWithTokensAndSessionMetadata()
    {
        var sessionRepositoryMock = new Mock<ISessionRepository>();
        var tokenRepositoryMock = new Mock<ITokenRepository>();
        var userManagerMock = CreateUserManagerMock();

        var commandDbContext = CreateCommandDbContext();
        var jwtSettings = CreateJwtSettings();
        var jwtOptions = Options.Create(jwtSettings);
        var jwtTokenProvider = new JwtTokenProvider(jwtOptions);
        var user = CreateUser();
        Session? capturedSession = null;

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
            jwtOptions,
            userManagerMock.Object);

        var result = await service.GenerateAuthTokensAsync(user, CreateCancellationToken());

        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.False(string.IsNullOrWhiteSpace(result.CsrfToken));
        Assert.Equal(user.Id, result.UserId);
        Assert.NotEqual(Guid.Empty, result.SessionId);
        Assert.True(result.RefreshTokenExpiresAt > result.AccessTokenExpiresAt);

        Assert.NotNull(capturedSession);
        Assert.Equal(user.Id, capturedSession!.UserId);
        Assert.False(string.IsNullOrWhiteSpace(capturedSession.RefreshTokenHash));
        Assert.False(string.IsNullOrWhiteSpace(capturedSession.RefreshTokenSalt));
        Assert.False(string.IsNullOrWhiteSpace(capturedSession.RefreshTokenPrefix));
        Assert.True(capturedSession.ExpiresAtUtc > capturedSession.CreatedAtUtc);
        Assert.Equal(result.SessionId, capturedSession.SessionId);

        sessionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Once);
        sessionRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task GenerateAuthTokensAsync_WhenUserHasRoles_ThenAccessTokenContainsRoleClaims()
    {
        var sessionRepositoryMock = new Mock<ISessionRepository>();
        var tokenRepositoryMock = new Mock<ITokenRepository>();
        var userManagerMock = CreateUserManagerMock();

        var commandDbContext = CreateCommandDbContext();
        var jwtSettings = CreateJwtSettings();
        var jwtOptions = Options.Create(jwtSettings);
        var jwtTokenProvider = new JwtTokenProvider(jwtOptions);
        var user = CreateUser();

        userManagerMock
            .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "admin", "Admin", " user ", "", "  " });

        sessionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        sessionRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new TokenService(
            sessionRepositoryMock.Object,
            tokenRepositoryMock.Object,
            commandDbContext,
            jwtTokenProvider,
            jwtOptions,
            userManagerMock.Object);

        var result = await service.GenerateAuthTokensAsync(user, CreateCancellationToken());

        var principal = jwtTokenProvider.ValidateToken(result.AccessToken);

        var roleClaims = principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        var roleAliasClaims = principal.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();

        Assert.Equal(2, roleClaims.Count);
        Assert.Contains("admin", roleClaims);
        Assert.Contains("user", roleClaims);

        Assert.Equal(2, roleAliasClaims.Count);
        Assert.Contains("admin", roleAliasClaims);
        Assert.Contains("user", roleAliasClaims);
    }

    [Fact]
    public async Task GetTokenInfoAsync_WhenSessionExists_ThenReturnsTokenInfo()
    {
        var sessionRepositoryMock = new Mock<ISessionRepository>();
        var tokenRepositoryMock = new Mock<ITokenRepository>();
        var userManagerMock = CreateUserManagerMock();

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
            jwtOptions,
            userManagerMock.Object);

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
        var userManagerMock = CreateUserManagerMock();

        var commandDbContext = CreateCommandDbContext();
        var jwtSettings = CreateJwtSettings();
        var jwtOptions = Options.Create(jwtSettings);
        var jwtTokenProvider = new JwtTokenProvider(jwtOptions);
        var sessionId = Guid.NewGuid();

        tokenRepositoryMock
            .Setup(x => x.GetTokenInfoAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenInfoDto?)null);

        var service = new TokenService(
            sessionRepositoryMock.Object,
            tokenRepositoryMock.Object,
            commandDbContext,
            jwtTokenProvider,
            jwtOptions,
            userManagerMock.Object);

        await Assert.ThrowsAsync<TokenException>(() => service.GetTokenInfoAsync(sessionId, CreateCancellationToken()));

        tokenRepositoryMock.Verify(x => x.GetTokenInfoAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokensAsync_WhenSessionIsRevoked_ThenThrowsTokenException()
    {
        var sessionRepositoryMock = new Mock<ISessionRepository>();
        var tokenRepositoryMock = new Mock<ITokenRepository>();
        var userManagerMock = CreateUserManagerMock();

        var commandDbContext = CreateCommandDbContext();
        var jwtSettings = CreateJwtSettings();
        var jwtOptions = Options.Create(jwtSettings);
        var jwtTokenProvider = new JwtTokenProvider(jwtOptions);

        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RefreshTokenHash = "h",
            RefreshTokenSalt = "s",
            RefreshTokenPrefix = "p",
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
            RevokedAtUtc = DateTime.UtcNow
        };

        var service = new TokenService(
            sessionRepositoryMock.Object,
            tokenRepositoryMock.Object,
            commandDbContext,
            jwtTokenProvider,
            jwtOptions,
            userManagerMock.Object);

        await Assert.ThrowsAsync<TokenException>(() => service.RefreshTokensAsync(session, CreateCancellationToken()));

        sessionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Never);
        sessionRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokensAsync_WhenSessionIsExpired_ThenThrowsTokenException()
    {
        var sessionRepositoryMock = new Mock<ISessionRepository>();
        var tokenRepositoryMock = new Mock<ITokenRepository>();
        var userManagerMock = CreateUserManagerMock();

        var commandDbContext = CreateCommandDbContext();
        var jwtSettings = CreateJwtSettings();
        var jwtOptions = Options.Create(jwtSettings);
        var jwtTokenProvider = new JwtTokenProvider(jwtOptions);

        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RefreshTokenHash = "h",
            RefreshTokenSalt = "s",
            RefreshTokenPrefix = "p",
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-30),
            ExpiresAtUtc = DateTime.UtcNow.AddSeconds(-1),
            RevokedAtUtc = null
        };

        var service = new TokenService(
            sessionRepositoryMock.Object,
            tokenRepositoryMock.Object,
            commandDbContext,
            jwtTokenProvider,
            jwtOptions,
            userManagerMock.Object);

        await Assert.ThrowsAsync<TokenException>(() => service.RefreshTokensAsync(session, CreateCancellationToken()));

        sessionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Never);
        sessionRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokensAsync_WhenUserNotFound_ThenThrowsTokenException()
    {
        var sessionRepositoryMock = new Mock<ISessionRepository>();
        var tokenRepositoryMock = new Mock<ITokenRepository>();
        var userManagerMock = CreateUserManagerMock();

        var commandDbContext = CreateCommandDbContext();
        var jwtSettings = CreateJwtSettings();
        var jwtOptions = Options.Create(jwtSettings);
        var jwtTokenProvider = new JwtTokenProvider(jwtOptions);

        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RefreshTokenHash = "h",
            RefreshTokenSalt = "s",
            RefreshTokenPrefix = "p",
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
            RevokedAtUtc = null
        };

        var service = new TokenService(
            sessionRepositoryMock.Object,
            tokenRepositoryMock.Object,
            commandDbContext,
            jwtTokenProvider,
            jwtOptions,
            userManagerMock.Object);

        await Assert.ThrowsAsync<TokenException>(() => service.RefreshTokensAsync(session, CreateCancellationToken()));

        sessionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Never);
        sessionRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokensAsync_WhenValidSession_ThenRevokesOldAndCreatesNewSessionAndReturnsTokens()
    {
        var sessionRepositoryMock = new Mock<ISessionRepository>();
        var tokenRepositoryMock = new Mock<ITokenRepository>();
        var userManagerMock = CreateUserManagerMock();

        var commandDbContext = CreateCommandDbContext();
        var jwtSettings = CreateJwtSettings();
        var jwtOptions = Options.Create(jwtSettings);
        var jwtTokenProvider = new JwtTokenProvider(jwtOptions);
        var user = CreateUser();

        userManagerMock
            .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "admin" });

        commandDbContext.ApplicationUsers.Add(user);
        await commandDbContext.SaveChangesAsync(CreateCancellationToken());

        var oldSession = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = user.Id,
            RefreshTokenHash = "h",
            RefreshTokenSalt = "s",
            RefreshTokenPrefix = "p",
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
            RevokedAtUtc = null
        };

        Session? capturedNew = null;

        sessionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .Callback<Session, CancellationToken>((s, _) => capturedNew = s)
            .Returns(Task.CompletedTask);

        sessionRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new TokenService(
            sessionRepositoryMock.Object,
            tokenRepositoryMock.Object,
            commandDbContext,
            jwtTokenProvider,
            jwtOptions,
            userManagerMock.Object);

        var result = await service.RefreshTokensAsync(oldSession, CreateCancellationToken());

        Assert.NotNull(oldSession.RevokedAtUtc);
        Assert.NotNull(oldSession.ReplacedBySessionId);
        Assert.NotNull(capturedNew);

        Assert.Equal(oldSession.ReplacedBySessionId, capturedNew!.SessionId);
        Assert.Equal(user.Id, capturedNew.UserId);
        Assert.False(string.IsNullOrWhiteSpace(capturedNew.RefreshTokenHash));
        Assert.False(string.IsNullOrWhiteSpace(capturedNew.RefreshTokenSalt));
        Assert.False(string.IsNullOrWhiteSpace(capturedNew.RefreshTokenPrefix));

        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(capturedNew.SessionId, result.SessionId);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.False(string.IsNullOrWhiteSpace(result.CsrfToken));
        Assert.True(result.RefreshTokenExpiresAt > result.AccessTokenExpiresAt);

        var principal = jwtTokenProvider.ValidateToken(result.AccessToken);
        Assert.Contains(principal.Claims, c => c.Type == ClaimTypes.Role && c.Value == "admin");
        Assert.Contains(principal.Claims, c => c.Type == "role" && c.Value == "admin");

        sessionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Once);
        sessionRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }
}
