using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Commands.User.Account.DeleteAccount;
using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.User.Commands.User.Account.DeleteAccount;

public sealed class DeleteAccountHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThenThrowsValidationException()
    {
        var userManager = CreateUserManagerMock();
        var validator = CreateValidatorMock<DeleteAccountDto>();
        using var db = CreateCommandDbContext();

        var handler = new DeleteAccountHandler(userManager.Object, validator.Object, db);

        await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(Guid.Empty, new DeleteAccountDto
            {
                CurrentPassword = "123456"
            }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsUserNotFoundException()
    {
        var userId = Guid.NewGuid();

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var validator = CreateValidatorMock<DeleteAccountDto>();
        using var db = CreateCommandDbContext();

        var handler = new DeleteAccountHandler(userManager.Object, validator.Object, db);

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(userId, new DeleteAccountDto
            {
                CurrentPassword = "123456"
            }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenPasswordInvalid_ThenThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId };

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        userManager.Setup(x => x.CheckPasswordAsync(user, "badpass"))
            .ReturnsAsync(false);

        var validator = CreateValidatorMock<DeleteAccountDto>();
        using var db = CreateCommandDbContext();

        var handler = new DeleteAccountHandler(userManager.Object, validator.Object, db);

        var ex = await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(userId, new DeleteAccountDto
            {
                CurrentPassword = "badpass"
            }, CancellationToken.None));

        Assert.Contains("Invalid password", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteFails_ThenThrowsValidationExceptionWithIdentityErrorDescription()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId };

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        userManager.Setup(x => x.CheckPasswordAsync(user, "goodpass"))
            .ReturnsAsync(true);

        userManager.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "failed" }));

        var validator = CreateValidatorMock<DeleteAccountDto>();
        using var db = CreateCommandDbContext();

        var handler = new DeleteAccountHandler(userManager.Object, validator.Object, db);

        var ex = await Assert.ThrowsAsync<AlgoDuck.Modules.User.Shared.Exceptions.ValidationException>(() =>
            handler.HandleAsync(userId, new DeleteAccountDto
            {
                CurrentPassword = "goodpass"
            }, CancellationToken.None));

        Assert.Contains("failed", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ThenCallsDeleteAndDoesNotThrow()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId };

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        userManager.Setup(x => x.CheckPasswordAsync(user, "goodpass"))
            .ReturnsAsync(true);

        userManager.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var validator = CreateValidatorMock<DeleteAccountDto>();
        using var db = CreateCommandDbContext();

        var handler = new DeleteAccountHandler(userManager.Object, validator.Object, db);

        await handler.HandleAsync(userId, new DeleteAccountDto
        {
            CurrentPassword = "goodpass"
        }, CancellationToken.None);

        userManager.Verify(x => x.DeleteAsync(user), Times.Once);
    }

    static ApplicationCommandDbContext CreateCommandDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
    }

    static Mock<IValidator<T>> CreateValidatorMock<T>()
    {
        var mock = new Mock<IValidator<T>>();

        mock.Setup(x => x.ValidateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        mock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<T>>(), It.IsAny<CancellationToken>()))
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
