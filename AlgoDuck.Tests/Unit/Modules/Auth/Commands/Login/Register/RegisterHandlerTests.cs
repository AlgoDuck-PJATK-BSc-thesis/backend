using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.Login.Register;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Shared.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using AuthValidationException = AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.Login.Register;

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
        var defaultDuckService = new Mock<IDefaultDuckService>();
        var config = CreateConfiguration("http://frontend");

        var handler = new RegisterHandler(
            userManager.Object,
            emailSender.Object,
            new RegisterValidator(),
            config,
            defaultDuckService.Object);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(new RegisterDto { UserName = "", Email = "", Password = "", ConfirmPassword = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUsernameTaken_ThrowsAuthValidationException()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var defaultDuckService = new Mock<IDefaultDuckService>();
        var config = CreateConfiguration("http://frontend");

        var handler = new RegisterHandler(
            userManager.Object,
            emailSender.Object,
            new RegisterValidator(),
            config,
            defaultDuckService.Object);

        var dto = new RegisterDto { UserName = "alice", Email = "alice@example.com", Password = "p", ConfirmPassword = "p" };

        userManager.Setup(x => x.FindByNameAsync(dto.UserName)).ReturnsAsync(new ApplicationUser { Id = Guid.NewGuid() });

        var ex = await Assert.ThrowsAsync<AuthValidationException>(() =>
            handler.HandleAsync(dto, CancellationToken.None));

        Assert.Equal("auth_validation_error", ex.Code);
        Assert.Equal("Username is already taken.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenRegistrationSucceeds_AssignsDefaultRole_EnsuresAlgoduckOwnedAndSelected_AndSendsEmail()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var defaultDuckService = new Mock<IDefaultDuckService>();
        var config = CreateConfiguration("http://frontend");

        var createdUserId = Guid.NewGuid();

        userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        userManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((u, _) => u.Id = createdUserId)
            .ReturnsAsync(IdentityResult.Success);

        defaultDuckService
            .Setup(x => x.EnsureAlgoduckOwnedAndSelectedAsync(createdUserId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "user")).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>())).ReturnsAsync("token");

        emailSender
            .Setup(x => x.SendEmailConfirmationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RegisterHandler(
            userManager.Object,
            emailSender.Object,
            new RegisterValidator(),
            config,
            defaultDuckService.Object);

        var dto = new RegisterDto { UserName = "bob", Email = "bob@example.com", Password = "p", ConfirmPassword = "p" };

        var result = await handler.HandleAsync(dto, CancellationToken.None);

        Assert.Equal(createdUserId, result.Id);

        defaultDuckService.Verify(
            x => x.EnsureAlgoduckOwnedAndSelectedAsync(createdUserId, It.IsAny<CancellationToken>()),
            Times.Once);

        userManager.Verify(
            x => x.AddToRoleAsync(It.Is<ApplicationUser>(u => u.Id == createdUserId), "user"),
            Times.Once);

        emailSender.Verify(
            x => x.SendEmailConfirmationAsync(createdUserId, "bob@example.com", It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
