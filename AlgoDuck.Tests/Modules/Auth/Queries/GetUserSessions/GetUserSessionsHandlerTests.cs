using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Queries.Sessions.GetUserSessions;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Services;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Queries.GetUserSessions;

public sealed class GetUserSessionsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThrowsTokenException()
    {
        var repo = new Mock<ISessionRepository>();
        var service = new SessionService(repo.Object);
        var handler = new GetUserSessionsHandler(service);

        var ex = await Assert.ThrowsAsync<TokenException>(() =>
            handler.HandleAsync(Guid.Empty, Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("token_error", ex.Code);
        Assert.Equal("User identifier is invalid.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenCurrentSessionIdEmpty_ThrowsTokenException()
    {
        var repo = new Mock<ISessionRepository>();
        var service = new SessionService(repo.Object);
        var handler = new GetUserSessionsHandler(service);

        var ex = await Assert.ThrowsAsync<TokenException>(() =>
            handler.HandleAsync(Guid.NewGuid(), Guid.Empty, CancellationToken.None));

        Assert.Equal("token_error", ex.Code);
        Assert.Equal("Session identifier is invalid.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_MapsAndOrdersSessions_AndMarksCurrent()
    {
        var repo = new Mock<ISessionRepository>();
        var service = new SessionService(repo.Object);
        var handler = new GetUserSessionsHandler(service);

        var userId = Guid.NewGuid();
        var currentSessionId = Guid.NewGuid();

        var s1 = new Session
        {
            SessionId = currentSessionId,
            UserId = userId,
            RefreshTokenHash = "h",
            RefreshTokenSalt = "s",
            CreatedAtUtc = new DateTime(2025, 1, 10, 10, 0, 0, DateTimeKind.Utc),
            ExpiresAtUtc = new DateTime(2025, 1, 11, 10, 0, 0, DateTimeKind.Utc),
            RevokedAtUtc = null
        };

        var s2 = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = userId,
            RefreshTokenHash = "h2",
            RefreshTokenSalt = "s2",
            CreatedAtUtc = new DateTime(2025, 1, 12, 10, 0, 0, DateTimeKind.Utc),
            ExpiresAtUtc = new DateTime(2025, 1, 13, 10, 0, 0, DateTimeKind.Utc),
            RevokedAtUtc = new DateTime(2025, 1, 12, 11, 0, 0, DateTimeKind.Utc)
        };

        repo.Setup(x => x.GetUserSessionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Session> { s1, s2 });

        var result = await handler.HandleAsync(userId, currentSessionId, CancellationToken.None);

        Assert.Equal(2, result.Count);

        Assert.Equal(s2.SessionId, result[0].SessionId);
        Assert.False(result[0].IsCurrent);
        Assert.Equal(TimeSpan.Zero, result[0].CreatedAt.Offset);
        Assert.Equal(TimeSpan.Zero, result[0].ExpiresAt.Offset);
        Assert.NotNull(result[0].RevokedAt);
        Assert.Equal(TimeSpan.Zero, result[0].RevokedAt!.Value.Offset);

        Assert.Equal(s1.SessionId, result[1].SessionId);
        Assert.True(result[1].IsCurrent);
        Assert.Null(result[1].RevokedAt);
    }
}
