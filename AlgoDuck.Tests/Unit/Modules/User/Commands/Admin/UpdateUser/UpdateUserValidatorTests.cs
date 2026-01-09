using AlgoDuck.Modules.User.Commands.Admin.UpdateUser;

namespace AlgoDuck.Tests.Unit.Modules.User.Commands.Admin.UpdateUser;

public sealed class UpdateUserValidatorTests
{
    [Fact]
    public void Validate_WhenAllFieldsNull_IsInvalid()
    {
        var v = new UpdateUserValidator();
        var result = v.Validate(new UpdateUserDto());
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenAllFieldsWhitespace_IsInvalid()
    {
        var v = new UpdateUserValidator();
        var dto = new UpdateUserDto
        {
            Username = "   ",
            Email = "   ",
            Password = "   ",
            Role = "   "
        };
        var result = v.Validate(dto);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenValidEmailOnly_IsValid()
    {
        var v = new UpdateUserValidator();
        var result = v.Validate(new UpdateUserDto { Email = "a@b.com" });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenInvalidEmail_IsInvalid()
    {
        var v = new UpdateUserValidator();
        var result = v.Validate(new UpdateUserDto { Email = "not-email" });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WhenPasswordTooShort_IsInvalid()
    {
        var v = new UpdateUserValidator();
        var result = v.Validate(new UpdateUserDto { Password = "short" });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_WhenRoleInvalid_IsInvalid()
    {
        var v = new UpdateUserValidator();
        var result = v.Validate(new UpdateUserDto { Role = "manager" });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Role");
    }

    [Fact]
    public void Validate_WhenRoleAdminWithWhitespace_IsValid()
    {
        var v = new UpdateUserValidator();
        var result = v.Validate(new UpdateUserDto { Role = "  ADMIN  " });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenUsernameTooShort_IsInvalid()
    {
        var v = new UpdateUserValidator();
        var result = v.Validate(new UpdateUserDto { Username = "ab" });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Username");
    }

    [Fact]
    public void Validate_WhenUsernameValid_IsValid()
    {
        var v = new UpdateUserValidator();
        var result = v.Validate(new UpdateUserDto { Username = "good_name" });
        Assert.True(result.IsValid);
    }
}
