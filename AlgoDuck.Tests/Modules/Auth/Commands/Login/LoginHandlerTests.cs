using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.Login;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Moq;
using AuthValidationException = AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException;

namespace AlgoDuck.Tests.Modules.Auth.Commands.Login;

public sealed class LoginHandlerTests
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task HandleAsync_WhenDtoIsInvalid_ThenThrowsFluentValidationException()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var twoFactorService = new Mock<ITwoFactorService>();

        var handler = new LoginHandler(userManager.Object, tokenService.Object, twoFactorService.Object, new LoginValidator());

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(new LoginDto { UserNameOrEmail = "", Password = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsAuthValidationException()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var twoFactorService = new Mock<ITwoFactorService>();

        var handler = new LoginHandler(userManager.Object, tokenService.Object, twoFactorService.Object, new LoginValidator());

        var dto = new LoginDto { UserNameOrEmail = "alice", Password = "Password123" };

        userManager.Setup(x => x.FindByNameAsync(dto.UserNameOrEmail)).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.FindByEmailAsync(dto.UserNameOrEmail)).ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<AuthValidationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenPasswordInvalid_ThenThrowsAuthValidationException()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var twoFactorService = new Mock<ITwoFactorService>();

        var handler = new LoginHandler(userManager.Object, tokenService.Object, twoFactorService.Object, new LoginValidator());

        var dto = new LoginDto { UserNameOrEmail = "alice", Password = "wrong" };

        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "alice", Email = "alice@gmail.com", EmailConfirmed = true };

        userManager.Setup(x => x.FindByNameAsync(dto.UserNameOrEmail)).ReturnsAsync(user);
        userManager.Setup(x => x.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(false);

        await Assert.ThrowsAsync<AuthValidationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenEmailNotConfirmed_ThenThrowsEmailVerificationException()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var twoFactorService = new Mock<ITwoFactorService>();

        var handler = new LoginHandler(userManager.Object, tokenService.Object, twoFactorService.Object, new LoginValidator());

        var dto = new LoginDto { UserNameOrEmail = "alice", Password = "Password123" };

        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "alice", Email = "alice@gmail.com", EmailConfirmed = false };

        userManager.Setup(x => x.FindByNameAsync(dto.UserNameOrEmail)).ReturnsAsync(user);
        userManager.Setup(x => x.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);

        await Assert.ThrowsAsync<EmailVerificationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenTwoFactorEnabled_ThenReturnsTwoFactorChallenge()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var twoFactorService = new Mock<ITwoFactorService>();

        var handler = new LoginHandler(userManager.Object, tokenService.Object, twoFactorService.Object, new LoginValidator());

        var dto = new LoginDto { UserNameOrEmail = "alice", Password = "Password123" };

        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "alice", Email = "alice@gmail.com", EmailConfirmed = true, TwoFactorEnabled = true };

        userManager.Setup(x => x.FindByNameAsync(dto.UserNameOrEmail)).ReturnsAsync(user);
        userManager.Setup(x => x.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);

        var challengeId = "challenge-123";
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(5);

        twoFactorService.Setup(x => x.SendLoginCodeAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync((challengeId, expiresAt));

        var result = await handler.HandleAsync(dto, CancellationToken.None);

        Assert.True(result.TwoFactorRequired);
        Assert.Null(result.Auth);
        Assert.Equal(challengeId, result.ChallengeId);
        Assert.Equal(expiresAt, result.ExpiresAt);
    }

    [Fact]
    public async Task HandleAsync_WhenTwoFactorDisabled_ThenReturnsAuthTokens()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var twoFactorService = new Mock<ITwoFactorService>();

        var handler = new LoginHandler(userManager.Object, tokenService.Object, twoFactorService.Object, new LoginValidator());

        var dto = new LoginDto { UserNameOrEmail = "alice", Password = "Password123" };

        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "alice", Email = "alice@gmail.com", EmailConfirmed = true, TwoFactorEnabled = false };

        userManager.Setup(x => x.FindByNameAsync(dto.UserNameOrEmail)).ReturnsAsync(user);
        userManager.Setup(x => x.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);

        var authResponse = new AuthResponse
        {
            AccessToken = "access-token",
            RefreshToken = "refresh-token",
            CsrfToken = "csrf-token",
            UserId = user.Id,
            SessionId = Guid.NewGuid(),
            AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
            RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        tokenService.Setup(x => x.GenerateAuthTokensAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        var result = await handler.HandleAsync(dto, CancellationToken.None);

        Assert.False(result.TwoFactorRequired);
        Assert.NotNull(result.Auth);
        Assert.Equal(authResponse.AccessToken, result.Auth!.AccessToken);
    }
}
