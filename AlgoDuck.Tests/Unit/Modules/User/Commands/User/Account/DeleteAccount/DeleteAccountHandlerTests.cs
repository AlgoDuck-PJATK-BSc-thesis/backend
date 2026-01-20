using AlgoDuck.Models;
using AlgoDuck.Modules.User.Commands.User.Account.DeleteAccount;
using AlgoDuck.Modules.User.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.User.Commands.User.Account.DeleteAccount;

public sealed class DeleteAccountHandlerTests
{
    private const string ConfirmationPhrase = "I am sure I want to delete my account";

    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThenThrowsValidationException()
    {
        var userManager = CreateUserManagerMock();
        var handler = new DeleteAccountHandler(userManager.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(Guid.Empty, new DeleteAccountDto { ConfirmationText = ConfirmationPhrase }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThenThrowsUserNotFoundException()
    {
        var userId = Guid.NewGuid();

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new DeleteAccountHandler(userManager.Object);

        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            handler.HandleAsync(userId, new DeleteAccountDto { ConfirmationText = ConfirmationPhrase }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenDeleteFails_ThenThrowsValidationExceptionWithIdentityErrorDescription()
    {
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId };

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        userManager.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "failed" }));

        var handler = new DeleteAccountHandler(userManager.Object);

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            handler.HandleAsync(userId, new DeleteAccountDto { ConfirmationText = ConfirmationPhrase }, CancellationToken.None));

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

        userManager.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var handler = new DeleteAccountHandler(userManager.Object);

        await handler.HandleAsync(userId, new DeleteAccountDto { ConfirmationText = ConfirmationPhrase }, CancellationToken.None);

        userManager.Verify(x => x.DeleteAsync(user), Times.Once);
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
