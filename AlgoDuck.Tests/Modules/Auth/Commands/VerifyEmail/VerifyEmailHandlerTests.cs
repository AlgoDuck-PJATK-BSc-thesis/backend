using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.Email.VerifyEmail;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailHandlerTests
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
        var handler = new VerifyEmailHandler(userManager.Object, new VerifyEmailValidator());

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(new VerifyEmailDto { UserId = Guid.Empty, Token = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsEmailVerificationException()
    {
        var userManager = CreateUserManagerMock();
        var handler = new VerifyEmailHandler(userManager.Object, new VerifyEmailValidator());

        var dto = new VerifyEmailDto { UserId = Guid.NewGuid(), Token = "t" };

        userManager.Setup(x => x.FindByIdAsync(dto.UserId.ToString())).ReturnsAsync((ApplicationUser?)null);

        var ex = await Assert.ThrowsAsync<EmailVerificationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));

        Assert.Equal("email_verification_error", ex.Code);
        Assert.Equal("User not found.", ex.Message);
    }
}