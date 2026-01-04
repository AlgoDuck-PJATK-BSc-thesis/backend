using AlgoDuck.Modules.Auth.Commands.Login.ExternalLogin;

namespace AlgoDuck.Tests.Modules.Auth.Commands.ExternalLogin;

public sealed class ExternalLoginValidatorTests
{
    [Fact]
    public void Validate_WhenProviderEmpty_Fails()
    {
        var v = new ExternalLoginValidator();

        var r = v.Validate(new ExternalLoginDto
        {
            Provider = "",
            ExternalUserId = "u",
            Email = "alice@example.com",
            DisplayName = "Alice"
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenExternalUserIdEmpty_Fails()
    {
        var v = new ExternalLoginValidator();

        var r = v.Validate(new ExternalLoginDto
        {
            Provider = "google",
            ExternalUserId = "",
            Email = "alice@example.com",
            DisplayName = "Alice"
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenEmailInvalid_Fails()
    {
        var v = new ExternalLoginValidator();

        var r = v.Validate(new ExternalLoginDto
        {
            Provider = "google",
            ExternalUserId = "u",
            Email = "not-an-email",
            DisplayName = "Alice"
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenDisplayNameEmpty_Fails()
    {
        var v = new ExternalLoginValidator();

        var r = v.Validate(new ExternalLoginDto
        {
            Provider = "google",
            ExternalUserId = "u",
            Email = "alice@example.com",
            DisplayName = ""
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_Passes()
    {
        var v = new ExternalLoginValidator();

        var r = v.Validate(new ExternalLoginDto
        {
            Provider = "google",
            ExternalUserId = "u",
            Email = "alice@example.com",
            DisplayName = "Alice"
        });

        Assert.True(r.IsValid);
    }
}