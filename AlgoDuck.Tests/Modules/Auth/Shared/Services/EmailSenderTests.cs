using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Services;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Services;

public class EmailSenderTests
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }

    [Fact]
    public async Task SendEmailConfirmationAsync_WhenCalled_ThenUsesEmailTransportWithRenderedTemplate()
    {
        var transportMock = new Mock<IEmailTransport>();
        var userManagerMock = CreateUserManagerMock();
        var sender = new EmailSender(transportMock.Object, userManagerMock.Object);
        var userId = Guid.NewGuid();
        var email = "alice@gmail.com";
        var confirmationLink = "https://algoduck.test/confirm-email?token=abc";

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "alice",
            Email = email
        };

        userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        transportMock
            .Setup(x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await sender.SendEmailConfirmationAsync(userId, email, confirmationLink, CancellationToken.None);

        transportMock.Verify(x => x.SendAsync(
            email,
            It.IsAny<string>(),
            It.IsAny<string>(),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetAsync_WhenCalled_ThenUsesEmailTransportWithRenderedTemplate()
    {
        var transportMock = new Mock<IEmailTransport>();
        var userManagerMock = CreateUserManagerMock();
        var sender = new EmailSender(transportMock.Object, userManagerMock.Object);
        var userId = Guid.NewGuid();
        var email = "alice@gmail.com";
        var resetLink = "https://algoduck.test/reset-password?token=xyz";

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "alice",
            Email = email
        };

        userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        transportMock
            .Setup(x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await sender.SendPasswordResetAsync(userId, email, resetLink, CancellationToken.None);

        transportMock.Verify(x => x.SendAsync(
            email,
            It.IsAny<string>(),
            It.IsAny<string>(),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendTwoFactorCodeAsync_WhenCalled_ThenUsesEmailTransportWithRenderedTemplate()
    {
        var transportMock = new Mock<IEmailTransport>();
        var userManagerMock = CreateUserManagerMock();
        var sender = new EmailSender(transportMock.Object, userManagerMock.Object);
        var userId = Guid.NewGuid();
        var email = "alice@gmail.com";
        var code = "123456";

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "alice",
            Email = email
        };

        userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        transportMock
            .Setup(x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await sender.SendTwoFactorCodeAsync(userId, email, code, CancellationToken.None);

        transportMock.Verify(x => x.SendAsync(
            email,
            It.IsAny<string>(),
            It.IsAny<string>(),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailChangeConfirmationAsync_WhenCalled_ThenUsesEmailTransportWithNewEmail()
    {
        var transportMock = new Mock<IEmailTransport>();
        var userManagerMock = CreateUserManagerMock();
        var sender = new EmailSender(transportMock.Object, userManagerMock.Object);
        var userId = Guid.NewGuid();
        var newEmail = "alice+new@gmail.com";
        var confirmationLink = "https://algoduck.test/confirm-email-change?token=def";

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "alice",
            Email = newEmail
        };

        userManagerMock
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);

        transportMock
            .Setup(x => x.SendAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await sender.SendEmailChangeConfirmationAsync(userId, newEmail, confirmationLink, CancellationToken.None);

        transportMock.Verify(x => x.SendAsync(
            newEmail,
            It.IsAny<string>(),
            It.IsAny<string>(),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
