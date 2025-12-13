using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.ExternalLogin;
using AlgoDuck.Modules.Auth.Shared.DTOs;
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
        var tokenService = new Mock<AlgoDuck.Modules.Auth.Shared.Interfaces.ITokenService>();

        var handler = new ExternalLoginHandler(userManager.Object, tokenService.Object, new ExternalLoginValidator());

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(new ExternalLoginDto { Provider = "", ExternalUserId = "", Email = "", DisplayName = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenProviderUnsupported_ThrowsValidationException()
    {
        var userManager = CreateUserManagerMock();
        var tokenService = new Mock<AlgoDuck.Modules.Auth.Shared.Interfaces.ITokenService>();

        var handler = new ExternalLoginHandler(userManager.Object, tokenService.Object, new ExternalLoginValidator());

        var ex = await Assert.ThrowsAsync<AuthValidationException>(() =>
            handler.HandleAsync(new ExternalLoginDto { Provider = "unknown", ExternalUserId = "u", Email = "alice@example.com", DisplayName = "Alice" }, CancellationToken.None));

        Assert.Equal("auth_validation_error", ex.Code);
        Assert.Equal("Unsupported external provider.", ex.Message);
    }
}