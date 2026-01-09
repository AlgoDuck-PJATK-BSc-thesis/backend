using AlgoDuck.Modules.Auth.Commands.Login.Logout;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.Login.Logout;

public sealed class LogoutValidatorTests
{
    [Fact]
    public void Validate_WhenSessionIdIsNull_IsValid()
    {
        var validator = new LogoutValidator();
        var dto = new LogoutDto { SessionId = null };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.SessionId);
    }

    [Fact]
    public void Validate_WhenSessionIdIsEmptyGuid_IsInvalid()
    {
        var validator = new LogoutValidator();
        var dto = new LogoutDto { SessionId = Guid.Empty };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.SessionId);
    }

    [Fact]
    public void Validate_WhenSessionIdIsNonEmptyGuid_IsValid()
    {
        var validator = new LogoutValidator();
        var dto = new LogoutDto { SessionId = Guid.NewGuid() };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.SessionId);
    }
}