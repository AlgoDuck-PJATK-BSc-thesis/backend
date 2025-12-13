using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.RequestPasswordReset;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetHandlerTests
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    static IConfiguration ConfigWithDevOrigin(string? url)
    {
        var dict = new Dictionary<string, string?> { ["CORS:DevOrigins:0"] = url };
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThrowsFluentValidationException()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var config = ConfigWithDevOrigin("http://frontend");

        var handler = new RequestPasswordResetHandler(userManager.Object, emailSender.Object, new RequestPasswordResetValidator(), config);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(new RequestPasswordResetDto { Email = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ReturnsWithoutSendingEmail()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var config = ConfigWithDevOrigin("http://frontend");

        var dto = new RequestPasswordResetDto { Email = "alice@example.com" };

        userManager.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser?)null);

        var handler = new RequestPasswordResetHandler(userManager.Object, emailSender.Object, new RequestPasswordResetValidator(), config);

        await handler.HandleAsync(dto, CancellationToken.None);

        userManager.Verify(x => x.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
        emailSender.Verify(x => x.SendPasswordResetAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
