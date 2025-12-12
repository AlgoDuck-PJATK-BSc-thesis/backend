using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.StartEmailVerification;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using AuthValidationException = AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException;

namespace AlgoDuck.Tests.Modules.Auth.Commands.StartEmailVerification;

public sealed class StartEmailVerificationHandlerTests
{
    static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    static IConfiguration CreateConfiguration(string? frontendBaseUrl = null)
    {
        var dict = new Dictionary<string, string?> { ["CORS:DevOrigins:0"] = frontendBaseUrl };
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThrowsFluentValidationException()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var config = CreateConfiguration("http://frontend");

        var handler = new StartEmailVerificationHandler(userManager.Object, emailSender.Object, new StartEmailVerificationValidator(), config);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(new StartEmailVerificationDto { Email = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenEmailNotRegistered_ThrowsAuthValidationException()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var config = CreateConfiguration("http://frontend");

        var dto = new StartEmailVerificationDto { Email = "alice@example.com" };

        userManager.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser?)null);

        var handler = new StartEmailVerificationHandler(userManager.Object, emailSender.Object, new StartEmailVerificationValidator(), config);

        var ex = await Assert.ThrowsAsync<AuthValidationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));

        Assert.Equal("auth_validation_error", ex.Code);
        Assert.Equal("Email address is not registered.", ex.Message);
    }
}
