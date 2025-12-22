using AlgoDuck.Modules.Auth.Commands.RevokeSession;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Services;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Commands.RevokeSession;

public sealed class RevokeSessionHandlerTests
{
    static (RevokeSessionHandler handler, Mock<ISessionRepository> repo) Create()
    {
        var repo = new Mock<ISessionRepository>();
        var service = new SessionService(repo.Object);
        var handler = new RevokeSessionHandler(service, new RevokeSessionValidator());
        return (handler, repo);
    }

    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThrowsFluentValidationException()
    {
        var (handler, _) = Create();

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), new RevokeSessionDto { SessionId = Guid.Empty }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThrowsTokenExceptionNotAuthenticated()
    {
        var (handler, _) = Create();

        var ex = await Assert.ThrowsAsync<TokenException>(() =>
            handler.HandleAsync(Guid.Empty, new RevokeSessionDto { SessionId = Guid.NewGuid() }, CancellationToken.None));

        Assert.Equal("token_error", ex.Code);
        Assert.Equal("User is not authenticated.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_DelegatesToSessionService_RevokesAndSaves()
    {
        var (handler, repo) = Create();

        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var session = new AlgoDuck.Models.Session
        {
            SessionId = sessionId,
            UserId = userId,
            RefreshTokenHash = "h",
            RefreshTokenSalt = "s",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
            RevokedAtUtc = null
        };

        repo.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await handler.HandleAsync(userId, new RevokeSessionDto { SessionId = sessionId }, CancellationToken.None);

        Assert.NotNull(session.RevokedAtUtc);
        repo.Verify(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenSessionAlreadyRevoked_DoesNotSave()
    {
        var (handler, repo) = Create();

        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var session = new AlgoDuck.Models.Session
        {
            SessionId = sessionId,
            UserId = userId,
            RefreshTokenHash = "h",
            RefreshTokenSalt = "s",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
            RevokedAtUtc = DateTime.UtcNow.AddMinutes(-1)
        };

        repo.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        await handler.HandleAsync(userId, new RevokeSessionDto { SessionId = sessionId }, CancellationToken.None);

        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenSessionOwnedByDifferentUser_ThrowsPermissionException()
    {
        var (handler, repo) = Create();

        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var session = new AlgoDuck.Models.Session
        {
            SessionId = sessionId,
            UserId = Guid.NewGuid(),
            RefreshTokenHash = "h",
            RefreshTokenSalt = "s",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
            RevokedAtUtc = null
        };

        repo.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var ex = await Assert.ThrowsAsync<PermissionException>(() =>
            handler.HandleAsync(userId, new RevokeSessionDto { SessionId = sessionId }, CancellationToken.None));

        Assert.Equal("permission_denied", ex.Code);
        Assert.Equal("You do not own this session.", ex.Message);
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenSessionNotFound_ThrowsTokenException()
    {
        var (handler, repo) = Create();

        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        repo.Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync((AlgoDuck.Models.Session?)null);

        var ex = await Assert.ThrowsAsync<TokenException>(() =>
            handler.HandleAsync(userId, new RevokeSessionDto { SessionId = sessionId }, CancellationToken.None));

        Assert.Equal("token_error", ex.Code);
        Assert.Equal("Session not found.", ex.Message);
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
