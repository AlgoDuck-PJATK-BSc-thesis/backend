using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.Password.ResetPassword;
using Microsoft.AspNetCore.Identity;
using Moq;
using AuthValidationException = AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.Password.ResetPassword;

public sealed class ResetPasswordHandlerTests
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
        var handler = new ResetPasswordHandler(userManager.Object, new ResetPasswordValidator());

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(new ResetPasswordDto { UserId = Guid.Empty, Token = "", Password = "", ConfirmPassword = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsAuthValidationException()
    {
        var userManager = CreateUserManagerMock();
        var handler = new ResetPasswordHandler(userManager.Object, new ResetPasswordValidator());

        var dto = new ResetPasswordDto { UserId = Guid.NewGuid(), Token = "t", Password = "p", ConfirmPassword = "p" };

        userManager.Setup(x => x.FindByIdAsync(dto.UserId.ToString())).ReturnsAsync((ApplicationUser?)null);

        var ex = await Assert.ThrowsAsync<AuthValidationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));

        Assert.Equal("auth_validation_error", ex.Code);
        Assert.Equal("User not found.", ex.Message);
    }
}