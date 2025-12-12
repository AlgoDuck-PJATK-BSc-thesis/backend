using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.Register;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using AuthValidationException = AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException;

namespace AlgoDuck.Tests.Modules.Auth.Commands.Register;

public sealed class RegisterHandlerTests
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

        var handler = new RegisterHandler(userManager.Object, emailSender.Object, new RegisterValidator(), config);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(new RegisterDto { UserName = "", Email = "", Password = "", ConfirmPassword = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUsernameTaken_ThrowsAuthValidationException()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var config = CreateConfiguration("http://frontend");

        var handler = new RegisterHandler(userManager.Object, emailSender.Object, new RegisterValidator(), config);

        var dto = new RegisterDto { UserName = "alice", Email = "alice@example.com", Password = "p", ConfirmPassword = "p" };

        userManager.Setup(x => x.FindByNameAsync(dto.UserName)).ReturnsAsync(new ApplicationUser { Id = Guid.NewGuid() });

        var ex = await Assert.ThrowsAsync<AuthValidationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));

        Assert.Equal("auth_validation_error", ex.Code);
        Assert.Equal("Username is already taken.", ex.Message);
    }
}
