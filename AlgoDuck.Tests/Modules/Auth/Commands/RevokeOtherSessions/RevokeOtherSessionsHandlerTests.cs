using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.RevokeOtherSessions;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Services;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Commands.RevokeOtherSessions;

public sealed class RevokeOtherSessionsHandlerTests
{
    static (RevokeOtherSessionsHandler handler, Mock<ISessionRepository> repo) Create()
    {
        var repo = new Mock<ISessionRepository>();
        var service = new SessionService(repo.Object);
        var handler = new RevokeOtherSessionsHandler(service, new RevokeOtherSessionsValidator());
        return (handler, repo);
    }

    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThrowsFluentValidationException()
    {
        var (handler, _) = Create();

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), new RevokeOtherSessionsDto { CurrentSessionId = Guid.Empty }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThrowsTokenExceptionNotAuthenticated()
    {
        var (handler, _) = Create();

        var ex = await Assert.ThrowsAsync<TokenException>(() =>
            handler.HandleAsync(Guid.Empty, new RevokeOtherSessionsDto { CurrentSessionId = Guid.NewGuid() }, CancellationToken.None));

        Assert.Equal("token_error", ex.Code);
        Assert.Equal("User is not authenticated.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenOnlyCurrentSessionExists_DoesNotSave()
    {
        var (handler, repo) = Create();

        var userId = Guid.NewGuid();
        var currentSessionId = Guid.NewGuid();

        var s = new Session
        {
            SessionId = currentSessionId,
            UserId = userId,
            RefreshTokenHash = "h",
            RefreshTokenSalt = "s",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
            RevokedAtUtc = null
        };

        repo.Setup(x => x.GetUserSessionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session> { s });

        await handler.HandleAsync(userId, new RevokeOtherSessionsDto { CurrentSessionId = currentSessionId }, CancellationToken.None);

        Assert.Null(s.RevokedAtUtc);
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenOtherSessionsNotRevoked_SetsRevokedAtAndSavesOnce()
    {
        var (handler, repo) = Create();

        var userId = Guid.NewGuid();
        var currentSessionId = Guid.NewGuid();

        var nowBefore = DateTime.UtcNow;

        var current = new Session
        {
            SessionId = currentSessionId,
            UserId = userId,
            RefreshTokenHash = "h",
            RefreshTokenSalt = "s",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
            RevokedAtUtc = null
        };

        var other1 = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = userId,
            RefreshTokenHash = "h1",
            RefreshTokenSalt = "s1",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
            RevokedAtUtc = null
        };

        var other2AlreadyRevoked = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = userId,
            RefreshTokenHash = "h2",
            RefreshTokenSalt = "s2",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-3),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
            RevokedAtUtc = DateTime.UtcNow.AddMinutes(-10)
        };

        repo.Setup(x => x.GetUserSessionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session> { current, other1, other2AlreadyRevoked });

        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await handler.HandleAsync(userId, new RevokeOtherSessionsDto { CurrentSessionId = currentSessionId }, CancellationToken.None);

        Assert.Null(current.RevokedAtUtc);
        Assert.NotNull(other1.RevokedAtUtc);
        Assert.True(other1.RevokedAtUtc!.Value >= nowBefore);
        Assert.NotNull(other2AlreadyRevoked.RevokedAtUtc);

        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenAllOtherSessionsAlreadyRevoked_DoesNotSave()
    {
        var (handler, repo) = Create();

        var userId = Guid.NewGuid();
        var currentSessionId = Guid.NewGuid();

        var current = new Session
        {
            SessionId = currentSessionId,
            UserId = userId,
            RefreshTokenHash = "h",
            RefreshTokenSalt = "s",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
            RevokedAtUtc = null
        };

        var other = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = userId,
            RefreshTokenHash = "h1",
            RefreshTokenSalt = "s1",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
            RevokedAtUtc = DateTime.UtcNow.AddMinutes(-10)
        };

        repo.Setup(x => x.GetUserSessionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session> { current, other });

        await handler.HandleAsync(userId, new RevokeOtherSessionsDto { CurrentSessionId = currentSessionId }, CancellationToken.None);

        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
