using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.Login.Logout;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Services;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Commands.Logout;

public sealed class LogoutHandlerTests
{
    static (LogoutHandler handler, Mock<ISessionRepository> repo) CreateHandler()
    {
        var repo = new Mock<ISessionRepository>();
        var service = new SessionService(repo.Object);

        var validator = new Mock<IValidator<LogoutDto>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<LogoutDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new LogoutHandler(service, validator.Object);
        return (handler, repo);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotAuthenticated_ThrowsPermissionException()
    {
        var (handler, _) = CreateHandler();

        var ex = await Assert.ThrowsAsync<PermissionException>(() =>
            handler.HandleAsync(new LogoutDto(), null, Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("permission_denied", ex.Code);
        Assert.Equal("User is not authenticated.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThrowsPermissionException()
    {
        var (handler, _) = CreateHandler();

        var ex = await Assert.ThrowsAsync<PermissionException>(() =>
            handler.HandleAsync(new LogoutDto(), Guid.Empty, Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("permission_denied", ex.Code);
        Assert.Equal("User is not authenticated.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenDtoSessionIdProvided_UsesIt()
    {
        var (handler, repo) = CreateHandler();

        var userId = Guid.NewGuid();
        var dtoSessionId = Guid.NewGuid();
        var session = new Session
        {
            SessionId = dtoSessionId,
            UserId = userId,
            RefreshTokenHash = "h",
            RefreshTokenSalt = "s",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
            RevokedAtUtc = null
        };

        repo.Setup(x => x.GetByIdAsync(dtoSessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await handler.HandleAsync(new LogoutDto { SessionId = dtoSessionId }, userId, Guid.NewGuid(), CancellationToken.None);

        Assert.NotNull(session.RevokedAtUtc);
        repo.Verify(x => x.GetByIdAsync(dtoSessionId, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenDtoSessionIdMissing_UsesCurrentSessionId()
    {
        var (handler, repo) = CreateHandler();

        var userId = Guid.NewGuid();
        var currentSessionId = Guid.NewGuid();

        var session = new Session
        {
            SessionId = currentSessionId,
            UserId = userId,
            RefreshTokenHash = "h",
            RefreshTokenSalt = "s",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(1),
            RevokedAtUtc = null
        };

        repo.Setup(x => x.GetByIdAsync(currentSessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        repo.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await handler.HandleAsync(new LogoutDto { SessionId = null }, userId, currentSessionId, CancellationToken.None);

        Assert.NotNull(session.RevokedAtUtc);
        repo.Verify(x => x.GetByIdAsync(currentSessionId, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenDtoSessionIdMissingAndCurrentSessionMissing_ThrowsTokenException()
    {
        var (handler, _) = CreateHandler();

        var ex = await Assert.ThrowsAsync<TokenException>(() =>
            handler.HandleAsync(new LogoutDto { SessionId = null }, Guid.NewGuid(), null, CancellationToken.None));

        Assert.Equal("token_error", ex.Code);
        Assert.Equal("Session identifier is missing.", ex.Message);
    }
}
