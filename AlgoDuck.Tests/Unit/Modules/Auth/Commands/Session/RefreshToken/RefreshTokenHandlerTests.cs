using AlgoDuck.DAL;
using AlgoDuck.Modules.Auth.Commands.Session.RefreshToken;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.Session.RefreshToken;

public sealed class RefreshTokenHandlerTests
{
    static ApplicationCommandDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
    }

    static (string refreshToken, string prefix) CreateRefreshToken(string? prefix = null)
    {
        var p = prefix ?? new string('a', 32);
        var token = p + new string('b', 32);
        return (token, p);
    }

    static Models.Session CreateSession(Guid userId, string refreshToken, string prefix, DateTime utcNow, bool revoked, bool expired)
    {
        var salt = HashingHelper.GenerateSalt();
        var hash = HashingHelper.HashPassword(refreshToken, salt);

        return new Models.Session
        {
            SessionId = Guid.NewGuid(),
            UserId = userId,
            RefreshTokenPrefix = prefix,
            RefreshTokenSalt = Convert.ToBase64String(salt),
            RefreshTokenHash = hash,
            CreatedAtUtc = utcNow.AddDays(-1),
            ExpiresAtUtc = expired ? utcNow.AddMinutes(-1) : utcNow.AddMinutes(10),
            RevokedAtUtc = revoked ? utcNow.AddMinutes(-5) : null
        };
    }

    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThrowsFluentValidationException()
    {
        await using var db = CreateDb();
        var tokenService = new Mock<ITokenService>();
        var handler = new RefreshTokenHandler(db, tokenService.Object, new RefreshTokenValidator());

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(new RefreshTokenDto { RefreshToken = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenRefreshTokenWhitespace_ThrowsFluentValidationException()
    {
        await using var db = CreateDb();
        var tokenService = new Mock<ITokenService>();
        var handler = new RefreshTokenHandler(db, tokenService.Object, new RefreshTokenValidator());

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(new RefreshTokenDto { RefreshToken = "   " }, CancellationToken.None));

        tokenService.Verify(x => x.RefreshTokensAsync(It.IsAny<Models.Session>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSessionFound_ThrowsTokenExceptionInvalid()
    {
        await using var db = CreateDb();
        var tokenService = new Mock<ITokenService>();
        var handler = new RefreshTokenHandler(db, tokenService.Object, new RefreshTokenValidator());

        var (token, _) = CreateRefreshToken();

        var ex = await Assert.ThrowsAsync<TokenException>(() =>
            handler.HandleAsync(new RefreshTokenDto { RefreshToken = token }, CancellationToken.None));

        Assert.Equal("token_error", ex.Code);
        Assert.Equal("Invalid refresh token.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenRevoked_ThrowsTokenExceptionRevoked()
    {
        await using var db = CreateDb();
        var tokenService = new Mock<ITokenService>();
        var handler = new RefreshTokenHandler(db, tokenService.Object, new RefreshTokenValidator());

        var utcNow = DateTime.UtcNow;
        var userId = Guid.NewGuid();

        var (token, prefix) = CreateRefreshToken();
        var session = CreateSession(userId, token, prefix, utcNow, revoked: true, expired: false);

        db.Sessions.Add(session);
        await db.SaveChangesAsync(CancellationToken.None);

        var ex = await Assert.ThrowsAsync<TokenException>(() =>
            handler.HandleAsync(new RefreshTokenDto { RefreshToken = token }, CancellationToken.None));

        Assert.Equal("token_error", ex.Code);
        Assert.Equal("Refresh token has been revoked.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenExpired_SetsRevokedAtAndThrowsTokenExceptionExpired()
    {
        await using var db = CreateDb();
        var tokenService = new Mock<ITokenService>();
        var handler = new RefreshTokenHandler(db, tokenService.Object, new RefreshTokenValidator());

        var utcNow = DateTime.UtcNow;
        var userId = Guid.NewGuid();

        var (token, prefix) = CreateRefreshToken();
        var session = CreateSession(userId, token, prefix, utcNow, revoked: false, expired: true);

        db.Sessions.Add(session);
        await db.SaveChangesAsync(CancellationToken.None);

        var ex = await Assert.ThrowsAsync<TokenException>(() =>
            handler.HandleAsync(new RefreshTokenDto { RefreshToken = token }, CancellationToken.None));

        Assert.Equal("token_error", ex.Code);
        Assert.Equal("Refresh token has expired.", ex.Message);
        Assert.NotNull(session.RevokedAtUtc);
    }
}
