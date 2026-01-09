using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.Email.StartEmailVerification;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.Email.StartEmailVerification;

public sealed class StartEmailVerificationHandlerTests
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    static IConfiguration CreateConfiguration(string? apiBaseUrl = null, string? frontendBaseUrl = null)
    {
        var dict = new Dictionary<string, string?>
        {
            ["App:PublicApiUrl"] = apiBaseUrl,
            ["App:FrontendUrl"] = frontendBaseUrl
        };

        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThrowsFluentValidationException()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var config = CreateConfiguration("http://api", "http://frontend");

        var handler = new StartEmailVerificationHandler(userManager.Object, emailSender.Object, new StartEmailVerificationValidator(), config);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(new StartEmailVerificationDto { Email = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenEmailNotRegistered_DoesNothing()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var config = CreateConfiguration("http://api", "http://frontend");

        var dto = new StartEmailVerificationDto { Email = "alice@example.com" };

        userManager.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser?)null);

        var handler = new StartEmailVerificationHandler(userManager.Object, emailSender.Object, new StartEmailVerificationValidator(), config);

        await handler.HandleAsync(dto, CancellationToken.None);

        emailSender.VerifyNoOtherCalls();
    }
}
