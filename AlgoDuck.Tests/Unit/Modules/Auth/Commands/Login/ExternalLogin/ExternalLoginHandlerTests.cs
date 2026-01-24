using System.Text.RegularExpressions;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.Login.ExternalLogin;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Moq;
using AuthValidationException = AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.Login.ExternalLogin;

public sealed class ExternalLoginHandlerTests
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThrowsFluentValidationException()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var twoFactorService = new Mock<ITwoFactorService>();
        var userBootstrapper = new Mock<IUserBootstrapperService>();

        var handler = new ExternalLoginHandler(
            userManager.Object,
            tokenService.Object,
            twoFactorService.Object,
            new ExternalLoginValidator(),
            userBootstrapper.Object);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(new ExternalLoginDto { Provider = "", ExternalUserId = "", Email = "", DisplayName = "" }, CancellationToken.None));

        userBootstrapper.Verify(
            x => x.EnsureUserInitializedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderUnsupported_ThrowsValidationException()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var twoFactorService = new Mock<ITwoFactorService>();
        var userBootstrapper = new Mock<IUserBootstrapperService>();

        var handler = new ExternalLoginHandler(
            userManager.Object,
            tokenService.Object,
            twoFactorService.Object,
            new ExternalLoginValidator(),
            userBootstrapper.Object);

        var ex = await Assert.ThrowsAsync<AuthValidationException>(() =>
            handler.HandleAsync(new ExternalLoginDto { Provider = "unknown", ExternalUserId = "u", Email = "alice@example.com", DisplayName = "Alice" }, CancellationToken.None));

        Assert.Equal("auth_validation_error", ex.Code);
        Assert.Equal("Unsupported external provider.", ex.Message);

        userBootstrapper.Verify(
            x => x.EnsureUserInitializedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenNewExternalUser_ThenCreatesUser_WithGeneratedUsername_EnsuresDefaultDuck_AndCreatesUserConfig()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();
        var twoFactorService = new Mock<ITwoFactorService>();
        var userBootstrapper = new Mock<IUserBootstrapperService>();

        var createdUserId = Guid.NewGuid();

        userManager.Setup(x => x.FindByEmailAsync("alice@example.com")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var capturedUsers = new List<ApplicationUser>();

        userManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>()))
            .Callback<ApplicationUser>(u =>
            {
                u.Id = createdUserId;
                capturedUsers.Add(u);
            })
            .ReturnsAsync(IdentityResult.Success);

        userManager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string>());
        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "user")).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.GetLoginsAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<UserLoginInfo>());
        userManager.Setup(x => x.AddLoginAsync(It.IsAny<ApplicationUser>(), It.IsAny<UserLoginInfo>())).ReturnsAsync(IdentityResult.Success);

        userBootstrapper
            .Setup(x => x.EnsureUserInitializedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        tokenService
            .Setup(x => x.GenerateAuthTokensAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResponse
            {
                AccessToken = "a",
                RefreshToken = "r",
                CsrfToken = "c",
                AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
                RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                SessionId = Guid.NewGuid(),
                UserId = createdUserId
            });

        var handler = new ExternalLoginHandler(
            userManager.Object,
            tokenService.Object,
            twoFactorService.Object,
            new ExternalLoginValidator(),
            userBootstrapper.Object);

        var result = await handler.HandleAsync(new ExternalLoginDto
        {
            Provider = "google",
            ExternalUserId = "ext",
            Email = "alice@example.com",
            DisplayName = "Alice Example"
        }, CancellationToken.None);

        Assert.False(result.TwoFactorRequired);
        Assert.NotNull(result.Auth);

        Assert.Single(capturedUsers);
        Assert.NotNull(capturedUsers[0].UserName);
        Assert.DoesNotContain("@", capturedUsers[0].UserName!);
        Assert.NotEqual("alice@example.com", capturedUsers[0].UserName);
        Assert.Matches(new Regex("^[a-z]+_[a-z]+_[0-9]{4,6}$"), capturedUsers[0].UserName!);

        userBootstrapper.Verify(
            x => x.EnsureUserInitializedAsync(createdUserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
