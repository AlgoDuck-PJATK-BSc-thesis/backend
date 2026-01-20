using AlgoDuck.Modules.Auth.Commands.Email.ChangeEmailConfirm;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.Email.ChangeEmailConfirm;

public sealed class ChangeEmailConfirmValidatorTests
{
    [Fact]
    public void Validate_WhenUserIdEmpty_Fails()
    {
        var v = new ChangeEmailConfirmValidator();

        var r = v.Validate(new ChangeEmailConfirmDto
        {
            UserId = Guid.Empty,
            NewEmail = "a@b.com",
            Token = "t"
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenNewEmailInvalid_Fails()
    {
        var v = new ChangeEmailConfirmValidator();

        var r = v.Validate(new ChangeEmailConfirmDto
        {
            UserId = Guid.NewGuid(),
            NewEmail = "not-an-email",
            Token = "t"
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenTokenEmpty_Fails()
    {
        var v = new ChangeEmailConfirmValidator();

        var r = v.Validate(new ChangeEmailConfirmDto
        {
            UserId = Guid.NewGuid(),
            NewEmail = "a@b.com",
            Token = ""
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_Passes()
    {
        var v = new ChangeEmailConfirmValidator();

        var r = v.Validate(new ChangeEmailConfirmDto
        {
            UserId = Guid.NewGuid(),
            NewEmail = "a@b.com",
            Token = "t"
        });

        Assert.True(r.IsValid);
    }
}