using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Validators;
using Moq;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Validators;

public sealed class AuthValidatorTests
{
    [Fact]
    public async Task ValidateRegistrationAsync_WhenUserNameMissing_ThenThrowsValidationException()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var emailValidator = new EmailValidator();
        var passwordValidator = new PasswordValidator();
        var validator = new AuthValidator(authRepositoryMock.Object, emailValidator, passwordValidator);

        await Assert.ThrowsAsync<ValidationException>(() =>
            validator.ValidateRegistrationAsync("", "alice@example.com", "StrongPassw0rd", CancellationToken.None));

        authRepositoryMock.Verify(x => x.FindByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        authRepositoryMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateRegistrationAsync_WhenUserNameTooLong_ThenThrowsValidationException()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var emailValidator = new EmailValidator();
        var passwordValidator = new PasswordValidator();
        var validator = new AuthValidator(authRepositoryMock.Object, emailValidator, passwordValidator);
        var userName = new string('a', 65);

        await Assert.ThrowsAsync<ValidationException>(() =>
            validator.ValidateRegistrationAsync(userName, "alice@example.com", "StrongPassw0rd", CancellationToken.None));

        authRepositoryMock.Verify(x => x.FindByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        authRepositoryMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateRegistrationAsync_WhenEmailInvalid_ThenThrowsValidationException()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var emailValidator = new EmailValidator();
        var passwordValidator = new PasswordValidator();
        var validator = new AuthValidator(authRepositoryMock.Object, emailValidator, passwordValidator);

        await Assert.ThrowsAsync<ValidationException>(() =>
            validator.ValidateRegistrationAsync("alice", "not-an-email", "StrongPassw0rd", CancellationToken.None));

        authRepositoryMock.Verify(x => x.FindByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        authRepositoryMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateRegistrationAsync_WhenPasswordInvalid_ThenThrowsValidationException()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var emailValidator = new EmailValidator();
        var passwordValidator = new PasswordValidator();
        var validator = new AuthValidator(authRepositoryMock.Object, emailValidator, passwordValidator);

        await Assert.ThrowsAsync<ValidationException>(() =>
            validator.ValidateRegistrationAsync("alice", "alice@example.com", "weakpass", CancellationToken.None));

        authRepositoryMock.Verify(x => x.FindByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        authRepositoryMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateRegistrationAsync_WhenUserNameTaken_ThenThrowsValidationException()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var emailValidator = new EmailValidator();
        var passwordValidator = new PasswordValidator();
        var validator = new AuthValidator(authRepositoryMock.Object, emailValidator, passwordValidator);

        authRepositoryMock
            .Setup(x => x.FindByUserNameAsync("alice", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser { Id = Guid.NewGuid(), UserName = "alice" });

        await Assert.ThrowsAsync<ValidationException>(() =>
            validator.ValidateRegistrationAsync("alice", "alice@example.com", "StrongPassw0rd", CancellationToken.None));

        authRepositoryMock.Verify(x => x.FindByUserNameAsync("alice", It.IsAny<CancellationToken>()), Times.Once);
        authRepositoryMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateRegistrationAsync_WhenEmailTaken_ThenThrowsValidationException()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var emailValidator = new EmailValidator();
        var passwordValidator = new PasswordValidator();
        var validator = new AuthValidator(authRepositoryMock.Object, emailValidator, passwordValidator);

        authRepositoryMock
            .Setup(x => x.FindByUserNameAsync("alice", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        authRepositoryMock
            .Setup(x => x.FindByEmailAsync("alice@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser { Id = Guid.NewGuid(), Email = "alice@example.com" });

        await Assert.ThrowsAsync<ValidationException>(() =>
            validator.ValidateRegistrationAsync("alice", "alice@example.com", "StrongPassw0rd", CancellationToken.None));

        authRepositoryMock.Verify(x => x.FindByUserNameAsync("alice", It.IsAny<CancellationToken>()), Times.Once);
        authRepositoryMock.Verify(x => x.FindByEmailAsync("alice@example.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateRegistrationAsync_WhenValidAndUnique_ThenDoesNotThrow()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var emailValidator = new EmailValidator();
        var passwordValidator = new PasswordValidator();
        var validator = new AuthValidator(authRepositoryMock.Object, emailValidator, passwordValidator);

        authRepositoryMock
            .Setup(x => x.FindByUserNameAsync("alice", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        authRepositoryMock
            .Setup(x => x.FindByEmailAsync("alice@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var ex = await Record.ExceptionAsync(() =>
            validator.ValidateRegistrationAsync("alice", "alice@example.com", "StrongPassw0rd", CancellationToken.None));

        Assert.Null(ex);

        authRepositoryMock.Verify(x => x.FindByUserNameAsync("alice", It.IsAny<CancellationToken>()), Times.Once);
        authRepositoryMock.Verify(x => x.FindByEmailAsync("alice@example.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoginAsync_WhenUserNameOrEmailMissing_ThenThrowsValidationException()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new AuthValidator(authRepositoryMock.Object, new EmailValidator(), new PasswordValidator());

        await Assert.ThrowsAsync<ValidationException>(() =>
            validator.ValidateLoginAsync("   ", "pass", CancellationToken.None));

        authRepositoryMock.Verify(x => x.FindByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        authRepositoryMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateLoginAsync_WhenPasswordMissing_ThenThrowsValidationException()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new AuthValidator(authRepositoryMock.Object, new EmailValidator(), new PasswordValidator());

        await Assert.ThrowsAsync<ValidationException>(() =>
            validator.ValidateLoginAsync("alice", "   ", CancellationToken.None));

        authRepositoryMock.Verify(x => x.FindByUserNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        authRepositoryMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateLoginAsync_WhenUserFoundByUserName_ThenDoesNotThrow()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new AuthValidator(authRepositoryMock.Object, new EmailValidator(), new PasswordValidator());

        authRepositoryMock
            .Setup(x => x.FindByUserNameAsync("alice", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser { Id = Guid.NewGuid(), UserName = "alice" });

        var ex = await Record.ExceptionAsync(() =>
            validator.ValidateLoginAsync("alice", "pass", CancellationToken.None));

        Assert.Null(ex);

        authRepositoryMock.Verify(x => x.FindByUserNameAsync("alice", It.IsAny<CancellationToken>()), Times.Once);
        authRepositoryMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateLoginAsync_WhenUserFoundByEmail_ThenDoesNotThrow()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new AuthValidator(authRepositoryMock.Object, new EmailValidator(), new PasswordValidator());

        authRepositoryMock
            .Setup(x => x.FindByUserNameAsync("alice@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        authRepositoryMock
            .Setup(x => x.FindByEmailAsync("alice@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser { Id = Guid.NewGuid(), Email = "alice@example.com" });

        var ex = await Record.ExceptionAsync(() =>
            validator.ValidateLoginAsync("alice@example.com", "pass", CancellationToken.None));

        Assert.Null(ex);

        authRepositoryMock.Verify(x => x.FindByUserNameAsync("alice@example.com", It.IsAny<CancellationToken>()), Times.Once);
        authRepositoryMock.Verify(x => x.FindByEmailAsync("alice@example.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateLoginAsync_WhenUserNotFound_ThenThrowsValidationException()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new AuthValidator(authRepositoryMock.Object, new EmailValidator(), new PasswordValidator());

        authRepositoryMock
            .Setup(x => x.FindByUserNameAsync("nope", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        authRepositoryMock
            .Setup(x => x.FindByEmailAsync("nope", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<ValidationException>(() =>
            validator.ValidateLoginAsync("nope", "pass", CancellationToken.None));

        authRepositoryMock.Verify(x => x.FindByUserNameAsync("nope", It.IsAny<CancellationToken>()), Times.Once);
        authRepositoryMock.Verify(x => x.FindByEmailAsync("nope", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void ValidateEmailConfirmationAsync_WhenUserIdEmpty_ThenThrowsEmailVerificationException()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new AuthValidator(authRepositoryMock.Object, new EmailValidator(), new PasswordValidator());

        Assert.Throws<EmailVerificationException>(() =>
            validator.ValidateEmailConfirmationAsync(Guid.Empty, "token", CancellationToken.None).GetAwaiter().GetResult());
    }

    [Fact]
    public void ValidateEmailConfirmationAsync_WhenTokenMissing_ThenThrowsEmailVerificationException()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new AuthValidator(authRepositoryMock.Object, new EmailValidator(), new PasswordValidator());

        Assert.Throws<EmailVerificationException>(() =>
            validator.ValidateEmailConfirmationAsync(Guid.NewGuid(), "   ", CancellationToken.None).GetAwaiter().GetResult());
    }

    [Fact]
    public async Task ValidateEmailConfirmationAsync_WhenValid_ThenCompletes()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new AuthValidator(authRepositoryMock.Object, new EmailValidator(), new PasswordValidator());

        var ex = await Record.ExceptionAsync(() =>
            validator.ValidateEmailConfirmationAsync(Guid.NewGuid(), "token", CancellationToken.None));

        Assert.Null(ex);
    }

    [Fact]
    public void ValidatePasswordChangeAsync_WhenUserIdEmpty_ThenThrowsValidationException()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new AuthValidator(authRepositoryMock.Object, new EmailValidator(), new PasswordValidator());

        Assert.Throws<ValidationException>(() =>
            validator.ValidatePasswordChangeAsync(Guid.Empty, "old", "NewPassw0rd", CancellationToken.None).GetAwaiter().GetResult());
    }

    [Fact]
    public void ValidatePasswordChangeAsync_WhenCurrentPasswordMissing_ThenThrowsValidationException()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new AuthValidator(authRepositoryMock.Object, new EmailValidator(), new PasswordValidator());

        Assert.Throws<ValidationException>(() =>
            validator.ValidatePasswordChangeAsync(Guid.NewGuid(), "   ", "NewPassw0rd", CancellationToken.None).GetAwaiter().GetResult());
    }

    [Fact]
    public async Task ValidatePasswordChangeAsync_WhenNewPasswordInvalid_ThenThrowsValidationException()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new AuthValidator(authRepositoryMock.Object, new EmailValidator(), new PasswordValidator());

        await Assert.ThrowsAsync<ValidationException>(() =>
            validator.ValidatePasswordChangeAsync(Guid.NewGuid(), "old", "weakpass", CancellationToken.None));
    }

    [Fact]
    public async Task ValidatePasswordChangeAsync_WhenValid_ThenCompletes()
    {
        var authRepositoryMock = new Mock<IAuthRepository>();
        var validator = new AuthValidator(authRepositoryMock.Object, new EmailValidator(), new PasswordValidator());

        var ex = await Record.ExceptionAsync(() =>
            validator.ValidatePasswordChangeAsync(Guid.NewGuid(), "old", "NewPassw0rd", CancellationToken.None));

        Assert.Null(ex);
    }
}
