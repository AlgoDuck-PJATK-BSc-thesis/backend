using AlgoDuck.Models;
using AlgoDuck.Modules.User.Commands.Admin.CreateUser;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.User.Commands.Admin.CreateUser;

public sealed class CreateUserHandlerTests
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
        var userBootstrapper = new Mock<IUserBootstrapperService>();
        var handler = new CreateUserHandler(userManager.Object, new CreateUserValidator(), userBootstrapper.Object);

        var dto = new CreateUserDto
        {
            Email = "",
            Password = "",
            Role = ""
        };

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));

        userBootstrapper.Verify(
            x => x.EnsureUserInitializedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailAlreadyExists_ThrowsValidationException()
    {
        var userManager = CreateUserManagerMock();
        var userBootstrapper = new Mock<IUserBootstrapperService>();
        var handler = new CreateUserHandler(userManager.Object, new CreateUserValidator(), userBootstrapper.Object);

        var dto = new CreateUserDto
        {
            Email = "alice@gmail.com",
            Password = "Password123",
            Role = "user",
            EmailVerified = true,
            Username = "alice_1"
        };

        userManager.Setup(x => x.FindByEmailAsync("alice@gmail.com")).ReturnsAsync(new ApplicationUser());

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));

        Assert.Equal("Email already exists.", ex.Message);

        userBootstrapper.Verify(
            x => x.EnsureUserInitializedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUsernameAlreadyExists_ThrowsValidationException()
    {
        var userManager = CreateUserManagerMock();
        var userBootstrapper = new Mock<IUserBootstrapperService>();
        var handler = new CreateUserHandler(userManager.Object, new CreateUserValidator(), userBootstrapper.Object);

        var dto = new CreateUserDto
        {
            Email = "new@gmail.com",
            Password = "Password123",
            Role = "user",
            EmailVerified = true,
            Username = "taken_name"
        };

        userManager.Setup(x => x.FindByEmailAsync("new@gmail.com")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.FindByNameAsync("taken_name")).ReturnsAsync(new ApplicationUser());

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));

        Assert.Equal("Username already exists.", ex.Message);

        userBootstrapper.Verify(
            x => x.EnsureUserInitializedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenCreateFails_ThrowsValidationExceptionWithIdentityErrorDescription()
    {
        var userManager = CreateUserManagerMock();
        var userBootstrapper = new Mock<IUserBootstrapperService>();
        var handler = new CreateUserHandler(userManager.Object, new CreateUserValidator(), userBootstrapper.Object);

        var dto = new CreateUserDto
        {
            Email = "new@gmail.com",
            Password = "Password123",
            Role = "user",
            EmailVerified = true,
            Username = "ok_name"
        };

        userManager.Setup(x => x.FindByEmailAsync("new@gmail.com")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.FindByNameAsync("ok_name")).ReturnsAsync((ApplicationUser?)null);

        var createResult = IdentityResult.Failed(new IdentityError { Description = "Create failed." });
        userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123")).ReturnsAsync(createResult);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));

        Assert.Equal("Create failed.", ex.Message);

        userBootstrapper.Verify(
            x => x.EnsureUserInitializedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenAddRoleFails_ThrowsValidationExceptionWithIdentityErrorDescription()
    {
        var userManager = CreateUserManagerMock();
        var userBootstrapper = new Mock<IUserBootstrapperService>();
        var handler = new CreateUserHandler(userManager.Object, new CreateUserValidator(), userBootstrapper.Object);

        var dto = new CreateUserDto
        {
            Email = "new@gmail.com",
            Password = "Password123",
            Role = "admin",
            EmailVerified = false,
            Username = "admin_user"
        };

        userManager.Setup(x => x.FindByEmailAsync("new@gmail.com")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.FindByNameAsync("admin_user")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123")).ReturnsAsync(IdentityResult.Success);

        var addRoleResult = IdentityResult.Failed(new IdentityError { Description = "Role failed." });
        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "admin")).ReturnsAsync(addRoleResult);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));

        Assert.Equal("Role failed.", ex.Message);

        userBootstrapper.Verify(
            x => x.EnsureUserInitializedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_AssignsRoleAndReturnsResult()
    {
        var userManager = CreateUserManagerMock();
        var userBootstrapper = new Mock<IUserBootstrapperService>();
        var handler = new CreateUserHandler(userManager.Object, new CreateUserValidator(), userBootstrapper.Object);

        var dto = new CreateUserDto
        {
            Email = " new@gmail.com ",
            Password = "Password123",
            Role = " Admin ",
            EmailVerified = false,
            Username = "some_admin"
        };

        userManager.Setup(x => x.FindByEmailAsync("new@gmail.com")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.FindByNameAsync("some_admin")).ReturnsAsync((ApplicationUser?)null);

        ApplicationUser? createdUser = null;

        userManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123"))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((u, _) => { createdUser = u; });

        userManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "admin"))
            .ReturnsAsync(IdentityResult.Success);

        userBootstrapper
            .Setup(x => x.EnsureUserInitializedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await handler.HandleAsync(dto, CancellationToken.None);

        Assert.NotNull(createdUser);
        Assert.Equal("some_admin", createdUser!.UserName);
        Assert.Equal("new@gmail.com", createdUser.Email);
        Assert.False(createdUser.EmailConfirmed);

        Assert.Equal(createdUser.Id, result.UserId);
        Assert.Equal("new@gmail.com", result.Email);
        Assert.Equal("some_admin", result.Username);
        Assert.Equal("admin", result.Role);
        Assert.False(result.EmailVerified);

        userBootstrapper.Verify(
            x => x.EnsureUserInitializedAsync(createdUser.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRoleIsNotAdmin_AssignsUserRole_AndEnsuresDefaultDuck()
    {
        var userManager = CreateUserManagerMock();
        var userBootstrapper = new Mock<IUserBootstrapperService>();
        var handler = new CreateUserHandler(userManager.Object, new CreateUserValidator(), userBootstrapper.Object);

        var dto = new CreateUserDto
        {
            Email = "user@gmail.com",
            Password = "Password123",
            Role = "user",
            EmailVerified = true,
            Username = "regular_user"
        };

        userManager.Setup(x => x.FindByEmailAsync("user@gmail.com")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.FindByNameAsync("regular_user")).ReturnsAsync((ApplicationUser?)null);

        ApplicationUser? createdUser = null;

        userManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123"))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((u, _) => { createdUser = u; });

        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "user")).ReturnsAsync(IdentityResult.Success);

        userBootstrapper
            .Setup(x => x.EnsureUserInitializedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await handler.HandleAsync(dto, CancellationToken.None);

        Assert.Equal("user", result.Role);

        Assert.NotNull(createdUser);

        userBootstrapper.Verify(
            x => x.EnsureUserInitializedAsync(createdUser!.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUsernameNotProvided_GeneratesAndUsesSomeUsername_AndEnsuresDefaultDuck()
    {
        var userManager = CreateUserManagerMock();
        var userBootstrapper = new Mock<IUserBootstrapperService>();
        var handler = new CreateUserHandler(userManager.Object, new CreateUserValidator(), userBootstrapper.Object);

        var dto = new CreateUserDto
        {
            Email = "user@gmail.com",
            Password = "Password123",
            Role = "user",
            EmailVerified = true,
            Username = null
        };

        userManager.Setup(x => x.FindByEmailAsync("user@gmail.com")).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        ApplicationUser? createdUser = null;

        userManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), "Password123"))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((u, _) => { createdUser = u; });

        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "user")).ReturnsAsync(IdentityResult.Success);

        userBootstrapper
            .Setup(x => x.EnsureUserInitializedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await handler.HandleAsync(dto, CancellationToken.None);

        Assert.NotNull(createdUser);
        Assert.False(string.IsNullOrWhiteSpace(createdUser!.UserName));
        Assert.Equal(createdUser.UserName, result.Username);

        userBootstrapper.Verify(
            x => x.EnsureUserInitializedAsync(createdUser.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
