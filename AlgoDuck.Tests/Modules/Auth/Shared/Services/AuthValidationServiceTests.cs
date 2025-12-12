using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Services;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Services;

public sealed class AuthValidationServiceTests
{
    [Fact]
    public async Task ValidateRegistrationAsync_DelegatesToValidator()
    {
        var validatorMock = new Mock<IAuthValidator>();
        var service = new AuthValidationService(validatorMock.Object);
        var ct = new CancellationTokenSource().Token;

        validatorMock
            .Setup(x => x.ValidateRegistrationAsync("user", "email@test.com", "pass", ct))
            .Returns(Task.CompletedTask);

        await service.ValidateRegistrationAsync("user", "email@test.com", "pass", ct);

        validatorMock.Verify(x => x.ValidateRegistrationAsync("user", "email@test.com", "pass", ct), Times.Once);
    }

    [Fact]
    public async Task ValidateLoginAsync_DelegatesToValidator()
    {
        var validatorMock = new Mock<IAuthValidator>();
        var service = new AuthValidationService(validatorMock.Object);
        var ct = new CancellationTokenSource().Token;

        validatorMock
            .Setup(x => x.ValidateLoginAsync("user", "pass", ct))
            .Returns(Task.CompletedTask);

        await service.ValidateLoginAsync("user", "pass", ct);

        validatorMock.Verify(x => x.ValidateLoginAsync("user", "pass", ct), Times.Once);
    }

    [Fact]
    public async Task ValidateEmailConfirmationAsync_DelegatesToValidator()
    {
        var validatorMock = new Mock<IAuthValidator>();
        var service = new AuthValidationService(validatorMock.Object);
        var userId = Guid.NewGuid();
        var ct = new CancellationTokenSource().Token;

        validatorMock
            .Setup(x => x.ValidateEmailConfirmationAsync(userId, "token", ct))
            .Returns(Task.CompletedTask);

        await service.ValidateEmailConfirmationAsync(userId, "token", ct);

        validatorMock.Verify(x => x.ValidateEmailConfirmationAsync(userId, "token", ct), Times.Once);
    }

    [Fact]
    public async Task ValidatePasswordChangeAsync_DelegatesToValidator()
    {
        var validatorMock = new Mock<IAuthValidator>();
        var service = new AuthValidationService(validatorMock.Object);
        var userId = Guid.NewGuid();
        var ct = new CancellationTokenSource().Token;

        validatorMock
            .Setup(x => x.ValidatePasswordChangeAsync(userId, "old", "new", ct))
            .Returns(Task.CompletedTask);

        await service.ValidatePasswordChangeAsync(userId, "old", "new", ct);

        validatorMock.Verify(x => x.ValidatePasswordChangeAsync(userId, "old", "new", ct), Times.Once);
    }
}
