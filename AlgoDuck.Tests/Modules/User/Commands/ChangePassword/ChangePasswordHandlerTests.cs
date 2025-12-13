using AlgoDuck.Models;
using AlgoDuck.Modules.User.Commands.ChangePassword;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AlgoDuck.Tests.Modules.User.Commands.ChangePassword;

public sealed class ChangePasswordHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThenThrowsValidationException()
    {
        var userManager = CreateUserManagerMock();
        var validator = CreateValidatorMock<ChangePasswordDto>(new ChangePasswordDto
        {
            CurrentPassword = "123456",
            NewPassword = "12345678"
        });

        var handler = new ChangePasswordHandler(userManager.Object, validator.Object);

        await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(Guid.Empty, new ChangePasswordDto
            {
                CurrentPassword = "123456",
                NewPassword = "12345678"
            }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsValidationException()
    {
        var userId = Guid.NewGuid();

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var validator = CreateValidatorMock<ChangePasswordDto>(new ChangePasswordDto
        {
            CurrentPassword = "123456",
            NewPassword = "12345678"
        });

        var handler = new ChangePasswordHandler(userManager.Object, validator.Object);

        await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(userId, new ChangePasswordDto
            {
                CurrentPassword = "123456",
                NewPassword = "12345678"
            }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenChangePasswordFails_ThenThrowsValidationExceptionWithIdentityErrorDescription()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId };

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        userManager.Setup(x => x.ChangePasswordAsync(user, "oldpass", "newpass123"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "failed" }));

        var validator = CreateValidatorMock<ChangePasswordDto>(new ChangePasswordDto
        {
            CurrentPassword = "oldpass",
            NewPassword = "newpass123"
        });

        var handler = new ChangePasswordHandler(userManager.Object, validator.Object);

        var ex = await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(userId, new ChangePasswordDto
            {
                CurrentPassword = "oldpass",
                NewPassword = "newpass123"
            }, CancellationToken.None));

        Assert.Contains("failed", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ThenCallsChangePasswordAndDoesNotThrow()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId };

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        userManager.Setup(x => x.ChangePasswordAsync(user, "oldpass", "newpass123"))
            .ReturnsAsync(IdentityResult.Success);

        var validator = CreateValidatorMock<ChangePasswordDto>(new ChangePasswordDto
        {
            CurrentPassword = "oldpass",
            NewPassword = "newpass123"
        });

        var handler = new ChangePasswordHandler(userManager.Object, validator.Object);

        await handler.HandleAsync(userId, new ChangePasswordDto
        {
            CurrentPassword = "oldpass",
            NewPassword = "newpass123"
        }, CancellationToken.None);

        userManager.Verify(x => x.ChangePasswordAsync(user, "oldpass", "newpass123"), Times.Once);
    }

    static Mock<IValidator<T>> CreateValidatorMock<T>(T dto)
    {
        var mock = new Mock<IValidator<T>>();
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