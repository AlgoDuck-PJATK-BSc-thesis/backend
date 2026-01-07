using AlgoDuck.Models;
using AlgoDuck.Modules.User.Commands.Admin.UpdateUser;
using AlgoDuck.Modules.User.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;
using UserValidationException = AlgoDuck.Modules.User.Shared.Exceptions.ValidationException;

namespace AlgoDuck.Tests.Unit.Modules.User.Commands.Admin.UpdateUser;

public sealed class UpdateUserHandlerTests
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThrowsValidationException()
    {
        var userManager = CreateUserManagerMock();
        var handler = new UpdateUserHandler(userManager.Object, new UpdateUserValidator());

        var ex = await Assert.ThrowsAsync<UserValidationException>(() =>
            handler.HandleAsync(Guid.Empty, new UpdateUserDto { Email = "a@b.com" }, CancellationToken.None));

        Assert.Equal("User identifier is invalid.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThrowsFluentValidationException()
    {
        var userManager = CreateUserManagerMock();
        var handler = new UpdateUserHandler(userManager.Object, new UpdateUserValidator());

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), new UpdateUserDto(), CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsUserNotFoundException()
    {
        var userManager = CreateUserManagerMock();
        var handler = new UpdateUserHandler(userManager.Object, new UpdateUserValidator());

        var userId = Guid.NewGuid();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((ApplicationUser?)null);

        var ex = await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(userId, new UpdateUserDto { Email = "a@b.com" }, CancellationToken.None));

        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenUsernameTakenByAnotherUser_ThrowsValidationException()
    {
        var userManager = CreateUserManagerMock();
        var handler = new UpdateUserHandler(userManager.Object, new UpdateUserValidator());

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, UserName = "old", Email = "old@b.com" };
        var other = new ApplicationUser { Id = Guid.NewGuid(), UserName = "taken", Email = "x@y.com" };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.FindByNameAsync("taken")).ReturnsAsync(other);

        var ex = await Assert.ThrowsAsync<UserValidationException>(() =>
            handler.HandleAsync(userId, new UpdateUserDto { Username = " taken " }, CancellationToken.None));

        Assert.Equal("Username already exists.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenSetUsernameFails_ThrowsValidationExceptionWithIdentityError()
    {
        var userManager = CreateUserManagerMock();
        var handler = new UpdateUserHandler(userManager.Object, new UpdateUserValidator());

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, UserName = "old", Email = "old@b.com" };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.FindByNameAsync("newname")).ReturnsAsync((ApplicationUser?)null);

        var failed = IdentityResult.Failed(new IdentityError { Description = "Set username failed." });
        userManager.Setup(x => x.SetUserNameAsync(user, "newname")).ReturnsAsync(failed);

        var ex = await Assert.ThrowsAsync<UserValidationException>(() =>
            handler.HandleAsync(userId, new UpdateUserDto { Username = "newname" }, CancellationToken.None));

        Assert.Equal("Set username failed.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailTakenByAnotherUser_ThrowsValidationException()
    {
        var userManager = CreateUserManagerMock();
        var handler = new UpdateUserHandler(userManager.Object, new UpdateUserValidator());

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, UserName = "u", Email = "old@b.com" };
        var other = new ApplicationUser { Id = Guid.NewGuid(), UserName = "x", Email = "taken@b.com" };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.FindByEmailAsync("taken@b.com")).ReturnsAsync(other);

        var ex = await Assert.ThrowsAsync<UserValidationException>(() =>
            handler.HandleAsync(userId, new UpdateUserDto { Email = " taken@b.com " }, CancellationToken.None));

        Assert.Equal("Email already exists.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenSetEmailFails_ThrowsValidationExceptionWithIdentityError()
    {
        var userManager = CreateUserManagerMock();
        var handler = new UpdateUserHandler(userManager.Object, new UpdateUserValidator());

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, UserName = "u", Email = "old@b.com" };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.FindByEmailAsync("new@b.com")).ReturnsAsync((ApplicationUser?)null);

        var failed = IdentityResult.Failed(new IdentityError { Description = "Set email failed." });
        userManager.Setup(x => x.SetEmailAsync(user, "new@b.com")).ReturnsAsync(failed);

        var ex = await Assert.ThrowsAsync<UserValidationException>(() =>
            handler.HandleAsync(userId, new UpdateUserDto { Email = "new@b.com" }, CancellationToken.None));

        Assert.Equal("Set email failed.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenResetPasswordFails_ThrowsValidationExceptionWithIdentityError()
    {
        var userManager = CreateUserManagerMock();
        var handler = new UpdateUserHandler(userManager.Object, new UpdateUserValidator());

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, UserName = "u", Email = "old@b.com" };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("token");

        var failed = IdentityResult.Failed(new IdentityError { Description = "Reset password failed." });
        userManager.Setup(x => x.ResetPasswordAsync(user, "token", "Password123")).ReturnsAsync(failed);

        var ex = await Assert.ThrowsAsync<UserValidationException>(() =>
            handler.HandleAsync(userId, new UpdateUserDto { Password = "Password123" }, CancellationToken.None));

        Assert.Equal("Reset password failed.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenRoleChangeRequiresRemoveAndRemoveFails_ThrowsValidationException()
    {
        var userManager = CreateUserManagerMock();
        var handler = new UpdateUserHandler(userManager.Object, new UpdateUserValidator());

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, UserName = "u", Email = "old@b.com" };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "admin" });

        var rmFailed = IdentityResult.Failed(new IdentityError { Description = "Remove roles failed." });
        userManager.Setup(x => x.RemoveFromRolesAsync(user, It.Is<IList<string>>(l => l.Count == 1 && l[0] == "admin"))).ReturnsAsync(rmFailed);

        var ex = await Assert.ThrowsAsync<UserValidationException>(() =>
            handler.HandleAsync(userId, new UpdateUserDto { Role = "user" }, CancellationToken.None));

        Assert.Equal("Remove roles failed.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenRoleChangeRequiresAddAndAddFails_ThrowsValidationException()
    {
        var userManager = CreateUserManagerMock();
        var handler = new UpdateUserHandler(userManager.Object, new UpdateUserValidator());

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, UserName = "u", Email = "old@b.com" };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());

        var addFailed = IdentityResult.Failed(new IdentityError { Description = "Add role failed." });
        userManager.Setup(x => x.AddToRoleAsync(user, "admin")).ReturnsAsync(addFailed);

        var ex = await Assert.ThrowsAsync<UserValidationException>(() =>
            handler.HandleAsync(userId, new UpdateUserDto { Role = "admin" }, CancellationToken.None));

        Assert.Equal("Add role failed.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_UpdatesAndReturnsResult()
    {
        var userManager = CreateUserManagerMock();
        var handler = new UpdateUserHandler(userManager.Object, new UpdateUserValidator());

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, UserName = "old_user", Email = "old@b.com" };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        userManager.Setup(x => x.FindByNameAsync("new_user")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.SetUserNameAsync(user, "new_user"))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((u, name) => u.UserName = name);

        userManager.Setup(x => x.FindByEmailAsync("new@b.com")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.SetEmailAsync(user, "new@b.com"))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((u, mail) => u.Email = mail);

        userManager.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("token");
        userManager.Setup(x => x.ResetPasswordAsync(user, "token", "Password123")).ReturnsAsync(IdentityResult.Success);

        userManager.SetupSequence(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "user" })
            .ReturnsAsync(new List<string> { "admin" });

        userManager.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IList<string>>())).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.AddToRoleAsync(user, "admin")).ReturnsAsync(IdentityResult.Success);

        var result = await handler.HandleAsync(userId, new UpdateUserDto
        {
            Username = " new_user ",
            Email = " new@b.com ",
            Password = "Password123",
            Role = " Admin "
        }, CancellationToken.None);

        Assert.Equal(userId, result.UserId);
        Assert.Equal("new@b.com", result.Email);
        Assert.Equal("new_user", result.Username);
        Assert.Contains("admin", result.Roles.Select(r => r.ToLowerInvariant()));
    }
}
