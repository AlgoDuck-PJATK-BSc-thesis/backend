using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.ExternalLogin;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Moq;
using AuthValidationException = AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException;

namespace AlgoDuck.Tests.Modules.Auth.Commands.ExternalLogin;

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

        var handler = new ExternalLoginHandler(userManager.Object, tokenService.Object, new ExternalLoginValidator());

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(new ExternalLoginDto { Provider = "", ExternalUserId = "", Email = "", DisplayName = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenProviderUnsupported_ThrowsValidationException()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();

        var handler = new ExternalLoginHandler(userManager.Object, tokenService.Object, new ExternalLoginValidator());

        var ex = await Assert.ThrowsAsync<AuthValidationException>(() =>
            handler.HandleAsync(new ExternalLoginDto { Provider = "unknown", ExternalUserId = "u", Email = "alice@example.com", DisplayName = "Alice" }, CancellationToken.None));

        Assert.Equal("auth_validation_error", ex.Code);
        Assert.Equal("Unsupported external provider.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenNewExternalUser_ThenCreatesUser_WithGeneratedUsername_NotEmail()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<ITokenService>();

        userManager.Setup(x => x.FindByEmailAsync("alice@example.com")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string>());
        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "user")).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.GetLoginsAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<UserLoginInfo>());
        userManager.Setup(x => x.AddLoginAsync(It.IsAny<ApplicationUser>(), It.IsAny<UserLoginInfo>())).ReturnsAsync(IdentityResult.Success);

        var capturedUsers = new List<ApplicationUser>();
        userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>()))
            .Callback<ApplicationUser>(u => capturedUsers.Add(u))
            .ReturnsAsync(IdentityResult.Success);

        tokenService.Setup(x => x.GenerateAuthTokensAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResponse
            {
                AccessToken = "a",
                RefreshToken = "r",
                CsrfToken = "c",
                AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
                RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                SessionId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            });

        var handler = new ExternalLoginHandler(userManager.Object, tokenService.Object, new ExternalLoginValidator());

        await handler.HandleAsync(new ExternalLoginDto
        {
            Provider = "google",
            ExternalUserId = "ext",
            Email = "alice@example.com",
            DisplayName = "Alice Example"
        }, CancellationToken.None);

        Assert.Single(capturedUsers);
        Assert.NotNull(capturedUsers[0].UserName);
        Assert.DoesNotContain("@", capturedUsers[0].UserName!);
        Assert.NotEqual("alice@example.com", capturedUsers[0].UserName);
    }
}
