using AlgoDuck.Modules.Auth.Commands.Login;
using AlgoDuck.Modules.Auth.Commands.Login.Login;

namespace AlgoDuck.Tests.Modules.Auth.Commands.Login;

public sealed class LoginValidatorTests
{
    [Fact]
    public void Validate_WhenUserNameOrEmailEmpty_Fails()
    {
        var validator = new LoginValidator();

        var result = validator.Validate(new LoginDto
        {
            UserNameOrEmail = "",
            Password = "x"
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenPasswordEmpty_Fails()
    {
        var validator = new LoginValidator();

        var result = validator.Validate(new LoginDto
        {
            UserNameOrEmail = "x",
            Password = ""
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenUserNameOrEmailTooLong_Fails()
    {
        var validator = new LoginValidator();
        var s = new string('a', 257);

        var result = validator.Validate(new LoginDto
        {
            UserNameOrEmail = s,
            Password = "x"
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenPasswordTooLong_Fails()
    {
        var validator = new LoginValidator();
        var s = new string('a', 257);

        var result = validator.Validate(new LoginDto
        {
            UserNameOrEmail = "x",
            Password = s
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_Passes()
    {
        var validator = new LoginValidator();

        var result = validator.Validate(new LoginDto
        {
            UserNameOrEmail = "alice",
            Password = "Password123"
        });

        Assert.True(result.IsValid);
    }
}