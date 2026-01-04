using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands.Email.ChangeEmailRequest;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using AuthValidationException = AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException;

namespace AlgoDuck.Tests.Modules.Auth.Commands.ChangeEmailRequest;

public sealed class ChangeEmailRequestHandlerTests
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

    static IConfiguration ConfigWithEnvStyleOrigin(string? url)
    {
        var dict = new Dictionary<string, string?> { ["CORS__DEVORIGINS__0"] = url };
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThrowsFluentValidationException()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var config = ConfigWithDevOrigin("http://frontend");

        var handler = new ChangeEmailRequestHandler(userManager.Object, emailSender.Object, new ChangeEmailRequestValidator(), config);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), new ChangeEmailRequestDto { NewEmail = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserIdEmpty_ThrowsAuthValidationException()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var config = ConfigWithDevOrigin("http://frontend");

        var handler = new ChangeEmailRequestHandler(userManager.Object, emailSender.Object, new ChangeEmailRequestValidator(), config);

        var ex = await Assert.ThrowsAsync<AuthValidationException>(() =>
            handler.HandleAsync(Guid.Empty, new ChangeEmailRequestDto { NewEmail = "new@example.com" }, CancellationToken.None));

        Assert.Equal("auth_validation_error", ex.Code);
        Assert.Equal("User identifier is invalid.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsAuthValidationException()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var config = ConfigWithDevOrigin("http://frontend");

        var userId = Guid.NewGuid();
        var dto = new ChangeEmailRequestDto { NewEmail = "new@example.com" };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync((ApplicationUser?)null);

        var handler = new ChangeEmailRequestHandler(userManager.Object, emailSender.Object, new ChangeEmailRequestValidator(), config);

        var ex = await Assert.ThrowsAsync<AuthValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        Assert.Equal("auth_validation_error", ex.Code);
        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenNewEmailAlreadyUsedByDifferentUser_ThrowsAuthValidationException()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var config = ConfigWithDevOrigin("http://frontend");

        var userId = Guid.NewGuid();
        var dto = new ChangeEmailRequestDto { NewEmail = "new@example.com" };

        var user = new ApplicationUser { Id = userId, Email = "old@example.com" };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.FindByEmailAsync(dto.NewEmail)).ReturnsAsync(new ApplicationUser { Id = Guid.NewGuid() });

        var handler = new ChangeEmailRequestHandler(userManager.Object, emailSender.Object, new ChangeEmailRequestValidator(), config);

        var ex = await Assert.ThrowsAsync<AuthValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        Assert.Equal("auth_validation_error", ex.Code);
        Assert.Equal("Email address is already in use.", ex.Message);

        userManager.Verify(x => x.GenerateChangeEmailTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        emailSender.Verify(x => x.SendEmailChangeConfirmationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenNewEmailUsedBySameUser_AllowsAndSends()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var config = ConfigWithDevOrigin("http://frontend");

        var userId = Guid.NewGuid();
        var dto = new ChangeEmailRequestDto { NewEmail = "same@example.com" };

        var user = new ApplicationUser { Id = userId, Email = "same@example.com" };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.FindByEmailAsync(dto.NewEmail)).ReturnsAsync(user);
        userManager.Setup(x => x.GenerateChangeEmailTokenAsync(user, dto.NewEmail)).ReturnsAsync("token+value");

        string? capturedLink = null;

        emailSender
            .Setup(x => x.SendEmailChangeConfirmationAsync(user.Id, dto.NewEmail, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, string, CancellationToken>((_, _, link, _) => capturedLink = link)
            .Returns(Task.CompletedTask);

        var handler = new ChangeEmailRequestHandler(userManager.Object, emailSender.Object, new ChangeEmailRequestValidator(), config);

        await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.NotNull(capturedLink);
        Assert.StartsWith("http://frontend/auth/confirm-email-change?", capturedLink);
        Assert.Contains("token=token%2Bvalue", capturedLink);
    }

    [Fact]
    public async Task HandleAsync_WhenDevOriginsMissing_UsesCORS__DEVORIGINS__0()
    {
        var userManager = CreateUserManagerMock();
        var emailSender = new Mock<IEmailSender>();
        var config = ConfigWithEnvStyleOrigin("http://env-frontend");

        var userId = Guid.NewGuid();
        var dto = new ChangeEmailRequestDto { NewEmail = "new@example.com" };

        var user = new ApplicationUser { Id = userId, Email = "old@example.com" };

        userManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        userManager.Setup(x => x.FindByEmailAsync(dto.NewEmail)).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.GenerateChangeEmailTokenAsync(user, dto.NewEmail)).ReturnsAsync("t");

        string? capturedLink = null;

        emailSender
            .Setup(x => x.SendEmailChangeConfirmationAsync(user.Id, dto.NewEmail, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, string, CancellationToken>((_, _, link, _) => capturedLink = link)
            .Returns(Task.CompletedTask);

        var handler = new ChangeEmailRequestHandler(userManager.Object, emailSender.Object, new ChangeEmailRequestValidator(), config);

        await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.NotNull(capturedLink);
        Assert.StartsWith("http://env-frontend/auth/confirm-email-change?", capturedLink);
    }
}
