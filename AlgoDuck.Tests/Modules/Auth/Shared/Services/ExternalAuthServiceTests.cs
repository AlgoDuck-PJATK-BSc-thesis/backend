using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Services;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Services;

public sealed class ExternalAuthServiceTests
{
    [Fact]
    public async Task LoginWithProviderAsync_WhenProviderIsEmpty_ThenThrowsValidationException()
    {
        var externalProviderMock = new Mock<IExternalAuthProvider>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var validatorMock = new Mock<IAuthValidator>();

        var service = new ExternalAuthService(
            externalProviderMock.Object,
            authRepositoryMock.Object,
            tokenServiceMock.Object,
            validatorMock.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.LoginWithProviderAsync("", "token", CancellationToken.None));
    }

    [Fact]
    public async Task LoginWithProviderAsync_WhenAccessTokenIsEmpty_ThenThrowsValidationException()
    {
        var externalProviderMock = new Mock<IExternalAuthProvider>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var validatorMock = new Mock<IAuthValidator>();

        var service = new ExternalAuthService(
            externalProviderMock.Object,
            authRepositoryMock.Object,
            tokenServiceMock.Object,
            validatorMock.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.LoginWithProviderAsync("google", "   ", CancellationToken.None));
    }

    [Fact]
    public async Task LoginWithProviderAsync_WhenExternalProviderReturnsNull_ThenThrowsValidationException()
    {
        var externalProviderMock = new Mock<IExternalAuthProvider>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var validatorMock = new Mock<IAuthValidator>();

        externalProviderMock
            .Setup(x => x.GetUserInfoAsync("google", "token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthUserDto?)null);

        var service = new ExternalAuthService(
            externalProviderMock.Object,
            authRepositoryMock.Object,
            tokenServiceMock.Object,
            validatorMock.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.LoginWithProviderAsync("google", "token", CancellationToken.None));
    }

    [Fact]
    public async Task LoginWithProviderAsync_WhenExternalUserHasNoEmail_ThenThrowsValidationException()
    {
        var externalProviderMock = new Mock<IExternalAuthProvider>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var validatorMock = new Mock<IAuthValidator>();

        externalProviderMock
            .Setup(x => x.GetUserInfoAsync("google", "token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthUserDto { Email = "" });

        var service = new ExternalAuthService(
            externalProviderMock.Object,
            authRepositoryMock.Object,
            tokenServiceMock.Object,
            validatorMock.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.LoginWithProviderAsync("google", "token", CancellationToken.None));
    }

    [Fact]
    public async Task LoginWithProviderAsync_WhenNoUserForEmail_ThenThrowsValidationException()
    {
        var externalProviderMock = new Mock<IExternalAuthProvider>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var validatorMock = new Mock<IAuthValidator>();

        var externalUser = new AuthUserDto { Email = "alice@gmail.com" };

        externalProviderMock
            .Setup(x => x.GetUserInfoAsync("google", "token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(externalUser);

        authRepositoryMock
            .Setup(x => x.FindByEmailAsync(externalUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var service = new ExternalAuthService(
            externalProviderMock.Object,
            authRepositoryMock.Object,
            tokenServiceMock.Object,
            validatorMock.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.LoginWithProviderAsync("google", "token", CancellationToken.None));
    }

    [Fact]
    public async Task LoginWithProviderAsync_WhenUserExists_ThenValidatesAndReturnsTokens()
    {
        var externalProviderMock = new Mock<IExternalAuthProvider>();
        var authRepositoryMock = new Mock<IAuthRepository>();
        var tokenServiceMock = new Mock<ITokenService>();
        var validatorMock = new Mock<IAuthValidator>();

        var ct = new CancellationTokenSource().Token;

        var externalUser = new AuthUserDto
        {
            Email = "alice@gmail.com",
            UserName = "alice",
            Id = Guid.NewGuid(),
            EmailConfirmed = true
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = externalUser.Email,
            UserName = externalUser.UserName
        };

        var response = new AuthResponse
        {
            AccessToken = "a",
            RefreshToken = "r",
            CsrfToken = "c",
            AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(60),
            SessionId = Guid.NewGuid(),
            UserId = user.Id
        };

        externalProviderMock
            .Setup(x => x.GetUserInfoAsync("google", "token", ct))
            .ReturnsAsync(externalUser);

        authRepositoryMock
            .Setup(x => x.FindByEmailAsync(externalUser.Email, ct))
            .ReturnsAsync(user);

        validatorMock
            .Setup(x => x.ValidateLoginAsync(externalUser.Email, string.Empty, ct))
            .Returns(Task.CompletedTask);

        tokenServiceMock
            .Setup(x => x.GenerateAuthTokensAsync(user, ct))
            .ReturnsAsync(response);

        var service = new ExternalAuthService(
            externalProviderMock.Object,
            authRepositoryMock.Object,
            tokenServiceMock.Object,
            validatorMock.Object);

        var result = await service.LoginWithProviderAsync("google", "token", ct);

        Assert.Equal(response.AccessToken, result.AccessToken);
        Assert.Equal(response.RefreshToken, result.RefreshToken);
        Assert.Equal(response.CsrfToken, result.CsrfToken);
        Assert.Equal(response.SessionId, result.SessionId);
        Assert.Equal(response.UserId, result.UserId);

        externalProviderMock.Verify(x => x.GetUserInfoAsync("google", "token", ct), Times.Once);
        authRepositoryMock.Verify(x => x.FindByEmailAsync(externalUser.Email, ct), Times.Once);
        validatorMock.Verify(x => x.ValidateLoginAsync(externalUser.Email, string.Empty, ct), Times.Once);
        tokenServiceMock.Verify(x => x.GenerateAuthTokensAsync(user, ct), Times.Once);
    }
}
