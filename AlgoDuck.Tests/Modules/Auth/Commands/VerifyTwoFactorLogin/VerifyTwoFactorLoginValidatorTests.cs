using AlgoDuck.Modules.Auth.Commands.TwoFactor.VerifyTwoFactorLogin;

namespace AlgoDuck.Tests.Modules.Auth.Commands.VerifyTwoFactorLogin;

public sealed class VerifyTwoFactorLoginValidatorTests
{
    [Fact]
    public void Validate_WhenChallengeIdEmpty_Fails()
    {
        var validator = new VerifyTwoFactorLoginValidator();

        var result = validator.Validate(new VerifyTwoFactorLoginDto
        {
            ChallengeId = "",
            Code = "123456"
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenCodeEmpty_Fails()
    {
        var validator = new VerifyTwoFactorLoginValidator();

        var result = validator.Validate(new VerifyTwoFactorLoginDto
        {
            ChallengeId = "c",
            Code = ""
        });

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("12345")]
    [InlineData("1234567")]
    public void Validate_WhenCodeLengthNotSix_Fails(string code)
    {
        var validator = new VerifyTwoFactorLoginValidator();

        var result = validator.Validate(new VerifyTwoFactorLoginDto
        {
            ChallengeId = "c",
            Code = code
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_Passes()
    {
        var validator = new VerifyTwoFactorLoginValidator();

        var result = validator.Validate(new VerifyTwoFactorLoginDto
        {
            ChallengeId = "c",
            Code = "123456"
        });

        Assert.True(result.IsValid);
    }
}