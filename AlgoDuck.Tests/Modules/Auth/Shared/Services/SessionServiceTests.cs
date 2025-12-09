using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Services;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Services;

public class SessionServiceTests
{
    [Fact]
    public async Task GetUserSessionsAsync_WhenUserIdIsValid_ThenReturnsSessionsFromRepository()
    {
        var repositoryMock = new Mock<ISessionRepository>();
        var userId = Guid.NewGuid();
        var sessions = new List<Session>
        {
            new Session { SessionId = Guid.NewGuid(), UserId = userId },
            new Session { SessionId = Guid.NewGuid(), UserId = userId }
        };

        repositoryMock
            .Setup(x => x.GetUserSessionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        var service = new SessionService(repositoryMock.Object);

        var result = await service.GetUserSessionsAsync(userId, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.All(result, s => Assert.Equal(userId, s.UserId));
        repositoryMock.Verify(x => x.GetUserSessionsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserSessionsAsync_WhenUserIdIsEmpty_ThenThrowsTokenException()
    {
        var repositoryMock = new Mock<ISessionRepository>();
        var service = new SessionService(repositoryMock.Object);

        await Assert.ThrowsAsync<TokenException>(() => service.GetUserSessionsAsync(Guid.Empty, CancellationToken.None));

        repositoryMock.Verify(x => x.GetUserSessionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeSessionAsync_WhenUserIdIsEmpty_ThenThrowsTokenException()
    {
        var repositoryMock = new Mock<ISessionRepository>();
        var service = new SessionService(repositoryMock.Object);

        await Assert.ThrowsAsync<TokenException>(() => service.RevokeSessionAsync(Guid.Empty, Guid.NewGuid(), CancellationToken.None));

        repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeSessionAsync_WhenSessionIdIsEmpty_ThenThrowsTokenException()
    {
        var repositoryMock = new Mock<ISessionRepository>();
        var service = new SessionService(repositoryMock.Object);

        await Assert.ThrowsAsync<TokenException>(() => service.RevokeSessionAsync(Guid.NewGuid(), Guid.Empty, CancellationToken.None));

        repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeSessionAsync_WhenSessionDoesNotExist_ThenThrowsTokenException()
    {
        var repositoryMock = new Mock<ISessionRepository>();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        repositoryMock
            .Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session)null);

        var service = new SessionService(repositoryMock.Object);

        await Assert.ThrowsAsync<TokenException>(() => service.RevokeSessionAsync(userId, sessionId, CancellationToken.None));

        repositoryMock.Verify(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeSessionAsync_WhenSessionBelongsToDifferentUser_ThenThrowsPermissionException()
    {
        var repositoryMock = new Mock<ISessionRepository>();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var session = new Session
        {
            SessionId = sessionId,
            UserId = otherUserId
        };

        repositoryMock
            .Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var service = new SessionService(repositoryMock.Object);

        await Assert.ThrowsAsync<PermissionException>(() => service.RevokeSessionAsync(userId, sessionId, CancellationToken.None));

        repositoryMock.Verify(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeSessionAsync_WhenSessionAlreadyRevoked_ThenDoesNotSaveChanges()
    {
        var repositoryMock = new Mock<ISessionRepository>();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var revokedAt = DateTime.UtcNow.AddMinutes(-5);

        var session = new Session
        {
            SessionId = sessionId,
            UserId = userId,
            RevokedAtUtc = revokedAt
        };

        repositoryMock
            .Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var service = new SessionService(repositoryMock.Object);

        await service.RevokeSessionAsync(userId, sessionId, CancellationToken.None);

        Assert.Equal(revokedAt, session.RevokedAtUtc);
        repositoryMock.Verify(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeSessionAsync_WhenSessionIsActive_ThenMarksRevokedAndSavesChanges()
    {
        var repositoryMock = new Mock<ISessionRepository>();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var session = new Session
        {
            SessionId = sessionId,
            UserId = userId,
            RevokedAtUtc = null
        };

        repositoryMock
            .Setup(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        repositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new SessionService(repositoryMock.Object);

        await service.RevokeSessionAsync(userId, sessionId, CancellationToken.None);

        Assert.NotNull(session.RevokedAtUtc);
        repositoryMock.Verify(x => x.GetByIdAsync(sessionId, It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeOtherSessionsAsync_WhenUserIdIsEmpty_ThenThrowsTokenException()
    {
        var repositoryMock = new Mock<ISessionRepository>();
        var service = new SessionService(repositoryMock.Object);

        await Assert.ThrowsAsync<TokenException>(() => service.RevokeOtherSessionsAsync(Guid.Empty, Guid.NewGuid(), CancellationToken.None));

        repositoryMock.Verify(x => x.GetUserSessionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeOtherSessionsAsync_WhenCurrentSessionIdIsEmpty_ThenThrowsTokenException()
    {
        var repositoryMock = new Mock<ISessionRepository>();
        var service = new SessionService(repositoryMock.Object);

        await Assert.ThrowsAsync<TokenException>(() => service.RevokeOtherSessionsAsync(Guid.NewGuid(), Guid.Empty, CancellationToken.None));

        repositoryMock.Verify(x => x.GetUserSessionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeOtherSessionsAsync_WhenNoOtherActiveSessions_ThenDoesNotSaveChanges()
    {
        var repositoryMock = new Mock<ISessionRepository>();
        var userId = Guid.NewGuid();
        var currentSessionId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var sessions = new List<Session>
        {
            new Session { SessionId = currentSessionId, UserId = userId, RevokedAtUtc = null },
            new Session { SessionId = Guid.NewGuid(), UserId = userId, RevokedAtUtc = now },
            new Session { SessionId = Guid.NewGuid(), UserId = userId, RevokedAtUtc = now }
        };

        repositoryMock
            .Setup(x => x.GetUserSessionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        var service = new SessionService(repositoryMock.Object);

        await service.RevokeOtherSessionsAsync(userId, currentSessionId, CancellationToken.None);

        repositoryMock.Verify(x => x.GetUserSessionsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeOtherSessionsAsync_WhenOtherActiveSessionsExist_ThenRevokesThemAndSavesChanges()
    {
        var repositoryMock = new Mock<ISessionRepository>();
        var userId = Guid.NewGuid();
        var currentSessionId = Guid.NewGuid();

        var activeOtherSession1 = new Session { SessionId = Guid.NewGuid(), UserId = userId, RevokedAtUtc = null };
        var activeOtherSession2 = new Session { SessionId = Guid.NewGuid(), UserId = userId, RevokedAtUtc = null };
        var currentSession = new Session { SessionId = currentSessionId, UserId = userId, RevokedAtUtc = null };

        var sessions = new List<Session>
        {
            currentSession,
            activeOtherSession1,
            activeOtherSession2
        };

        repositoryMock
            .Setup(x => x.GetUserSessionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessions);

        repositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new SessionService(repositoryMock.Object);

        await service.RevokeOtherSessionsAsync(userId, currentSessionId, CancellationToken.None);

        Assert.Null(currentSession.RevokedAtUtc);
        Assert.NotNull(activeOtherSession1.RevokedAtUtc);
        Assert.NotNull(activeOtherSession2.RevokedAtUtc);

        repositoryMock.Verify(x => x.GetUserSessionsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
