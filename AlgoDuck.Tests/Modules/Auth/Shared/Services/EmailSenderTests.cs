using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Services;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Services;

public class EmailSenderTests
{
    [Fact]
    public async Task SendEmailConfirmationAsync_WhenCalled_ThenUsesEmailTransportWithRenderedTemplate()
    {
        var transportMock = new Mock<IEmailTransport>();
        var sender = new EmailSender(transportMock.Object);
        var userId = Guid.NewGuid();
        var email = "alice@gmail.com";
        var confirmationLink = "https://algoduck.test/confirm-email?token=abc";

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
        var sender = new EmailSender(transportMock.Object);
        var userId = Guid.NewGuid();
        var email = "alice@gmail.com";
        var resetLink = "https://algoduck.test/reset-password?token=xyz";

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
        var sender = new EmailSender(transportMock.Object);
        var userId = Guid.NewGuid();
        var email = "alice@gmail.com";
        var code = "123456";

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
        var sender = new EmailSender(transportMock.Object);
        var userId = Guid.NewGuid();
        var newEmail = "alice+new@gmail.com";
        var confirmationLink = "https://algoduck.test/confirm-email-change?token=def";

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
