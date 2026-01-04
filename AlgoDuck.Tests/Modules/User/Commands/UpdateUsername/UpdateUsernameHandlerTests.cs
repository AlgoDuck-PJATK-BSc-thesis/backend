using AlgoDuck.Models;
using AlgoDuck.Modules.User.Commands.User.Profile.UpdateUsername;
using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AlgoDuck.Tests.Modules.User.Commands.UpdateUsername;

public sealed class UpdateUsernameHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThenThrowsValidationException()
    {
        var userManager = CreateUserManagerMock();

        var dto = new UpdateUsernameDto { NewUserName = "newname" };
        var validator = CreateValidatorMock(dto);

        var handler = new UpdateUsernameHandler(userManager.Object, validator.Object);

        await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(Guid.Empty, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsUserNotFoundException()
    {
        var userId = Guid.NewGuid();

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var dto = new UpdateUsernameDto { NewUserName = "newname" };
        var validator = CreateValidatorMock(dto);

        var handler = new UpdateUsernameHandler(userManager.Object, validator.Object);

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUsernameTakenByOtherUser_ThenThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var currentUser = new ApplicationUser { Id = userId, UserName = "old" };
        var otherUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "newname" };

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(currentUser);

        userManager.Setup(x => x.FindByNameAsync("newname"))
            .ReturnsAsync(otherUser);

        var dto = new UpdateUsernameDto { NewUserName = "newname" };
        var validator = CreateValidatorMock(dto);

        var handler = new UpdateUsernameHandler(userManager.Object, validator.Object);

        var ex = await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        Assert.Contains("already taken", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ThenThrowsValidationExceptionWithIdentityErrorDescription()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, UserName = "old" };

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        userManager.Setup(x => x.FindByNameAsync("newname"))
            .ReturnsAsync((ApplicationUser?)null);

        userManager.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "failed" }));

        var dto = new UpdateUsernameDto { NewUserName = "newname" };
        var validator = CreateValidatorMock(dto);

        var handler = new UpdateUsernameHandler(userManager.Object, validator.Object);

        var ex = await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        Assert.Contains("failed", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ThenUpdatesUsernameAndDoesNotThrow()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, UserName = "old" };

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        userManager.Setup(x => x.FindByNameAsync("newname"))
            .ReturnsAsync((ApplicationUser?)null);

        userManager.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var dto = new UpdateUsernameDto { NewUserName = "newname" };
        var validator = CreateValidatorMock(dto);

        var handler = new UpdateUsernameHandler(userManager.Object, validator.Object);

        await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.Equal("newname", user.UserName);
        userManager.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    static Mock<IValidator<UpdateUsernameDto>> CreateValidatorMock(UpdateUsernameDto dto)
    {
        var mock = new Mock<IValidator<UpdateUsernameDto>>();
        mock.Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return mock;
    }

    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            Options.Create(new IdentityOptions()),
            new Mock<IPasswordHasher<ApplicationUser>>().Object,
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new Mock<ILookupNormalizer>().Object,
            new IdentityErrorDescriber(),
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object);
    }
}