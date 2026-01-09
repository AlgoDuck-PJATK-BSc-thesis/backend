using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.TwoFactor.EnableTwoFactor;
using Microsoft.AspNetCore.Identity;
using Moq;
using AuthPermissionException = AlgoDuck.Modules.Auth.Shared.Exceptions.PermissionException;
using AuthValidationException = AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.TwoFactor.EnableTwoFactor;

public sealed class EnableTwoFactorHandlerTests
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThrowsPermissionException()
    {
        var userManager = CreateUserManagerMock();
        var handler = new EnableTwoFactorHandler(userManager.Object);

        var ex = await Assert.ThrowsAsync<AuthPermissionException>(() =>
            handler.HandleAsync(Guid.Empty, CancellationToken.None));

        Assert.Equal("permission_denied", ex.Code);
        Assert.Equal("User is not authenticated.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsPermissionException()
    {
        var userManager = CreateUserManagerMock();
        var handler = new EnableTwoFactorHandler(userManager.Object);

        var userId = Guid.NewGuid();

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((ApplicationUser?)null);

        var ex = await Assert.ThrowsAsync<AuthPermissionException>(() =>
            handler.HandleAsync(userId, CancellationToken.None));

        Assert.Equal("permission_denied", ex.Code);
        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadyEnabled_DoesNothing()
    {
        var userManager = CreateUserManagerMock();
        var handler = new EnableTwoFactorHandler(userManager.Object);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, TwoFactorEnabled = true };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        await handler.HandleAsync(userId, CancellationToken.None);

        userManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ThrowsAuthValidationException()
    {
        var userManager = CreateUserManagerMock();
        var handler = new EnableTwoFactorHandler(userManager.Object);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, TwoFactorEnabled = false };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        userManager
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "bad1" }));

        var ex = await Assert.ThrowsAsync<AuthValidationException>(() =>
            handler.HandleAsync(userId, CancellationToken.None));

        Assert.Equal("auth_validation_error", ex.Code);
        Assert.Contains("Could not enable two-factor authentication:", ex.Message);
        Assert.Contains("bad1", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccess_SetsTwoFactorEnabledAndUpdates()
    {
        var userManager = CreateUserManagerMock();
        var handler = new EnableTwoFactorHandler(userManager.Object);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, TwoFactorEnabled = false };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        userManager
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        await handler.HandleAsync(userId, CancellationToken.None);

        Assert.True(user.TwoFactorEnabled);
        userManager.Verify(x => x.UpdateAsync(user), Times.Once);
    }
}
