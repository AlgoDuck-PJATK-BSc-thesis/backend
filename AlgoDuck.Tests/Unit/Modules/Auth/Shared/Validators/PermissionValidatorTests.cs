using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Validators;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Validators;

public sealed class PermissionValidatorTests
{
    [Fact]
    public void ValidatePermissionName_WhenNull_ThenThrowsValidationException()
    {
        var validator = new PermissionValidator();

        Assert.Throws<ValidationException>(() => validator.ValidatePermissionName(null!));
    }

    [Fact]
    public void ValidatePermissionName_WhenEmpty_ThenThrowsValidationException()
    {
        var validator = new PermissionValidator();

        Assert.Throws<ValidationException>(() => validator.ValidatePermissionName(""));
    }

    [Fact]
    public void ValidatePermissionName_WhenWhitespace_ThenThrowsValidationException()
    {
        var validator = new PermissionValidator();

        Assert.Throws<ValidationException>(() => validator.ValidatePermissionName("   "));
    }

    [Fact]
    public void ValidatePermissionName_WhenValid_ThenDoesNotThrow()
    {
        var validator = new PermissionValidator();

        var ex = Record.Exception(() => validator.ValidatePermissionName("auth.manage"));

        Assert.Null(ex);
    }
}