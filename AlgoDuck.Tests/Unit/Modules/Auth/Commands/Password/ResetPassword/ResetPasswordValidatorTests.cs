using AlgoDuck.Modules.Auth.Commands.Password.ResetPassword;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.Password.ResetPassword;

public sealed class ResetPasswordValidatorTests
{
    [Fact]
    public void Validate_WhenUserIdEmpty_Fails()
    {
        var v = new ResetPasswordValidator();

        var r = v.Validate(new ResetPasswordDto
        {
            UserId = Guid.Empty,
            Token = "t",
            Password = "p",
            ConfirmPassword = "p"
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenTokenEmpty_Fails()
    {
        var v = new ResetPasswordValidator();

        var r = v.Validate(new ResetPasswordDto
        {
            UserId = Guid.NewGuid(),
            Token = "",
            Password = "p",
            ConfirmPassword = "p"
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenPasswordEmpty_Fails()
    {
        var v = new ResetPasswordValidator();

        var r = v.Validate(new ResetPasswordDto
        {
            UserId = Guid.NewGuid(),
            Token = "t",
            Password = "",
            ConfirmPassword = ""
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenPasswordTooLong_Fails()
    {
        var v = new ResetPasswordValidator();
        var p = new string('a', 257);

        var r = v.Validate(new ResetPasswordDto
        {
            UserId = Guid.NewGuid(),
            Token = "t",
            Password = p,
            ConfirmPassword = p
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenConfirmPasswordMismatch_FailsWithMessage()
    {
        var v = new ResetPasswordValidator();

        var r = v.Validate(new ResetPasswordDto
        {
            UserId = Guid.NewGuid(),
            Token = "t",
            Password = "p1",
            ConfirmPassword = "p2"
        });

        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.ErrorMessage == "Password and confirmation password do not match.");
    }

    [Fact]
    public void Validate_WhenValid_Passes()
    {
        var v = new ResetPasswordValidator();

        var r = v.Validate(new ResetPasswordDto
        {
            UserId = Guid.NewGuid(),
            Token = "t",
            Password = "p",
            ConfirmPassword = "p"
        });

        Assert.True(r.IsValid);
    }
}
