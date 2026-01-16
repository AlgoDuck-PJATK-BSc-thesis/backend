using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Utils;
using AlgoDuck.Modules.User.Shared.Services;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.User.Shared.Services;

public sealed class ReminderEmailSenderTests
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task SendStudyReminderAsync_WhenUserHasUserName_UsesUserNameInTemplate_AndSendsEmail()
    {
        var userId = Guid.NewGuid();
        var email = "u1@test.local";
        var ct = new CancellationTokenSource().Token;

        var transport = new Mock<IEmailTransport>(MockBehavior.Strict);
        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(new ApplicationUser { Id = userId, UserName = "u1" });

        var expected = EmailTemplateRenderer.RenderStudyReminder("u1");

        transport
            .Setup(x => x.SendAsync(email, expected.Subject, expected.Body, null, ct))
            .Returns(Task.CompletedTask);

        var sut = new ReminderEmailSender(transport.Object, userManager.Object);

        await sut.SendStudyReminderAsync(userId, email, ct);

        userManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
        transport.Verify(x => x.SendAsync(email, expected.Subject, expected.Body, null, ct), Times.Once);
        transport.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SendStudyReminderAsync_WhenUserMissing_UsesEmailFallback_AndSendsEmail()
    {
        var userId = Guid.NewGuid();
        var email = "fallback@test.local";
        var ct = new CancellationTokenSource().Token;

        var transport = new Mock<IEmailTransport>(MockBehavior.Strict);
        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var expected = EmailTemplateRenderer.RenderStudyReminder(email);

        transport
            .Setup(x => x.SendAsync(email, expected.Subject, expected.Body, null, ct))
            .Returns(Task.CompletedTask);

        var sut = new ReminderEmailSender(transport.Object, userManager.Object);

        await sut.SendStudyReminderAsync(userId, email, ct);

        userManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
        transport.Verify(x => x.SendAsync(email, expected.Subject, expected.Body, null, ct), Times.Once);
        transport.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SendStudyReminderAsync_WhenUserNameWhitespace_UsesEmailFallback_AndSendsEmail()
    {
        var userId = Guid.NewGuid();
        var email = "fallback2@test.local";
        var ct = new CancellationTokenSource().Token;

        var transport = new Mock<IEmailTransport>(MockBehavior.Strict);
        var userManager = CreateUserManagerMock();

        userManager
            .Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(new ApplicationUser { Id = userId, UserName = "   " });

        var expected = EmailTemplateRenderer.RenderStudyReminder(email);

        transport
            .Setup(x => x.SendAsync(email, expected.Subject, expected.Body, null, ct))
            .Returns(Task.CompletedTask);

        var sut = new ReminderEmailSender(transport.Object, userManager.Object);

        await sut.SendStudyReminderAsync(userId, email, ct);

        userManager.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
        transport.Verify(x => x.SendAsync(email, expected.Subject, expected.Body, null, ct), Times.Once);
        transport.VerifyNoOtherCalls();
    }
}
