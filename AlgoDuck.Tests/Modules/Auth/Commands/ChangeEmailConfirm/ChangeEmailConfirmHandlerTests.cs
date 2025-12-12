using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.ChangeEmailConfirm;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Commands.ChangeEmailConfirm;

public sealed class ChangeEmailConfirmHandlerTests
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
        var handler = new ChangeEmailConfirmHandler(userManager.Object, new ChangeEmailConfirmValidator());

        var dto = new ChangeEmailConfirmDto { UserId = Guid.Empty, NewEmail = "", Token = "" };

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsEmailVerificationException()
    {
        var userManager = CreateUserManagerMock();
        var handler = new ChangeEmailConfirmHandler(userManager.Object, new ChangeEmailConfirmValidator());

        var dto = new ChangeEmailConfirmDto { UserId = Guid.NewGuid(), NewEmail = "new@example.com", Token = "t" };

        userManager.Setup(x => x.FindByIdAsync(dto.UserId.ToString())).ReturnsAsync((ApplicationUser?)null);

        var ex = await Assert.ThrowsAsync<EmailVerificationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));

        Assert.Equal("email_verification_error", ex.Code);
        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenChangeEmailFails_ThrowsEmailVerificationExceptionWithErrors()
    {
        var userManager = CreateUserManagerMock();
        var handler = new ChangeEmailConfirmHandler(userManager.Object, new ChangeEmailConfirmValidator());

        var dto = new ChangeEmailConfirmDto { UserId = Guid.NewGuid(), NewEmail = "new@example.com", Token = "t" };

        var user = new ApplicationUser { Id = dto.UserId, Email = "old@example.com", EmailConfirmed = false };

        userManager.Setup(x => x.FindByIdAsync(dto.UserId.ToString())).ReturnsAsync(user);

        userManager
            .Setup(x => x.ChangeEmailAsync(user, dto.NewEmail, dto.Token))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "bad1" }, new IdentityError { Description = "bad2" }));

        var ex = await Assert.ThrowsAsync<EmailVerificationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));

        Assert.Equal("email_verification_error", ex.Code);
        Assert.Contains("Email change verification failed:", ex.Message);
        Assert.Contains("bad1", ex.Message);
        Assert.Contains("bad2", ex.Message);

        userManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUpdateFails_ThrowsEmailVerificationExceptionWithErrors()
    {
        var userManager = CreateUserManagerMock();
        var handler = new ChangeEmailConfirmHandler(userManager.Object, new ChangeEmailConfirmValidator());

        var dto = new ChangeEmailConfirmDto { UserId = Guid.NewGuid(), NewEmail = "new@example.com", Token = "t" };

        var user = new ApplicationUser { Id = dto.UserId, Email = "old@example.com", EmailConfirmed = false };

        userManager.Setup(x => x.FindByIdAsync(dto.UserId.ToString())).ReturnsAsync(user);

        userManager
            .Setup(x => x.ChangeEmailAsync(user, dto.NewEmail, dto.Token))
            .Callback<ApplicationUser, string, string>((u, email, _) => u.Email = email)
            .ReturnsAsync(IdentityResult.Success);

        userManager
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "bad1" }));

        var ex = await Assert.ThrowsAsync<EmailVerificationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));

        Assert.Equal("email_verification_error", ex.Code);
        Assert.Contains("Failed to finalize email change:", ex.Message);
        Assert.Contains("bad1", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccess_ChangesEmail_SetsConfirmed_AndUpdates()
    {
        var userManager = CreateUserManagerMock();
        var handler = new ChangeEmailConfirmHandler(userManager.Object, new ChangeEmailConfirmValidator());

        var dto = new ChangeEmailConfirmDto { UserId = Guid.NewGuid(), NewEmail = "new@example.com", Token = "t" };

        var user = new ApplicationUser { Id = dto.UserId, Email = "old@example.com", EmailConfirmed = false };

        userManager.Setup(x => x.FindByIdAsync(dto.UserId.ToString())).ReturnsAsync(user);

        userManager
            .Setup(x => x.ChangeEmailAsync(user, dto.NewEmail, dto.Token))
            .Callback<ApplicationUser, string, string>((u, email, _) => u.Email = email)
            .ReturnsAsync(IdentityResult.Success);

        userManager
            .Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        await handler.HandleAsync(dto, CancellationToken.None);

        Assert.True(user.EmailConfirmed);

        userManager.Verify(x => x.ChangeEmailAsync(user, dto.NewEmail, dto.Token), Times.Once);
        userManager.Verify(x => x.UpdateAsync(user), Times.Once);
    }
}
