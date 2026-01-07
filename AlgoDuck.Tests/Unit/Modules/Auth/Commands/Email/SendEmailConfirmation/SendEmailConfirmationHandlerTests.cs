using System.Text;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.Email.SendEmailConfirmation;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.Email.SendEmailConfirmation;

public sealed class SendEmailConfirmationHandlerTests
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_DoesNothing()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var handler = new SendEmailConfirmationHandler(userManager.Object, emailSender.Object);

        var userId = Guid.NewGuid();
        var cmd = new SendEmailConfirmationCommand(userId);

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((ApplicationUser?)null);

        await handler.Handle(cmd, CancellationToken.None);

        userManager.Verify(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
        emailSender.Verify(x => x.SendEmailConfirmationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserEmailEmpty_DoesNothing()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var handler = new SendEmailConfirmationHandler(userManager.Object, emailSender.Object);

        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, Email = " " };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        await handler.Handle(new SendEmailConfirmationCommand(userId), CancellationToken.None);

        userManager.Verify(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
        emailSender.Verify(x => x.SendEmailConfirmationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSuccess_GeneratesToken_AndSendsEmailWithConfirmationUrl()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var handler = new SendEmailConfirmationHandler(userManager.Object, emailSender.Object);

        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var user = new ApplicationUser { Id = userId, Email = email };

        var token = "token-value-123";
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var expectedUrl = $"http://localhost:8080/auth/email-verification?userId={userId}&token={encodedToken}";

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(user)).ReturnsAsync(token);

        await handler.Handle(new SendEmailConfirmationCommand(userId), CancellationToken.None);

        userManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
        userManager.Verify(x => x.GenerateEmailConfirmationTokenAsync(user), Times.Once);

        emailSender.Verify(
            x => x.SendEmailConfirmationAsync(
                userId,
                email,
                expectedUrl,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
