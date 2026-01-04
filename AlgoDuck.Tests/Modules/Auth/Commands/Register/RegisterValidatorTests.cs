using AlgoDuck.Modules.Auth.Commands.Login.Register;

namespace AlgoDuck.Tests.Modules.Auth.Commands.Register;

public sealed class RegisterValidatorTests
{
    [Fact]
    public void Validate_WhenUserNameEmpty_Fails()
    {
        var v = new RegisterValidator();

        var r = v.Validate(new RegisterDto
        {
            UserName = "",
            Email = "a@b.com",
            Password = "p",
            ConfirmPassword = "p"
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenEmailInvalid_Fails()
    {
        var v = new RegisterValidator();

        var r = v.Validate(new RegisterDto
        {
            UserName = "alice",
            Email = "not-an-email",
            Password = "p",
            ConfirmPassword = "p"
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenPasswordEmpty_Fails()
    {
        var v = new RegisterValidator();

        var r = v.Validate(new RegisterDto
        {
            UserName = "alice",
            Email = "a@b.com",
            Password = "",
            ConfirmPassword = ""
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenConfirmPasswordDoesNotMatch_FailsWithMessage()
    {
        var v = new RegisterValidator();

        var r = v.Validate(new RegisterDto
        {
            UserName = "alice",
            Email = "a@b.com",
            Password = "p1",
            ConfirmPassword = "p2"
        });

        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.ErrorMessage == "Password and confirmation password do not match.");
    }

    [Fact]
    public void Validate_WhenValid_Passes()
    {
        var v = new RegisterValidator();

        var r = v.Validate(new RegisterDto
        {
            UserName = "alice",
            Email = "a@b.com",
            Password = "p",
            ConfirmPassword = "p"
        });

        Assert.True(r.IsValid);
    }
}