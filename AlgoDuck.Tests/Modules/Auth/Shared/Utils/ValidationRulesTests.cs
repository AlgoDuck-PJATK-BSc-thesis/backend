using AlgoDuck.Modules.Auth.Shared.Utils;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Utils;

public sealed class ValidationRulesTests
{
    [Fact]
    public void Constants_HaveExpectedValues()
    {
        Assert.Equal(8, ValidationRules.PasswordMinLength);
        Assert.Equal(128, ValidationRules.PasswordMaxLength);
        Assert.Equal(64, ValidationRules.UserNameMaxLength);
        Assert.Equal(256, ValidationRules.EmailMaxLength);
    }

    [Theory]
    [InlineData("alice@example.com")]
    [InlineData("ALICE@EXAMPLE.COM")]
    [InlineData("alice.smith+test@example.co.uk")]
    [InlineData("alice_smith@example.io")]
    public void EmailRegex_MatchesValidEmails(string email)
    {
        Assert.True(ValidationRules.EmailRegex.IsMatch(email));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("alice")]
    [InlineData("alice@")]
    [InlineData("@example.com")]
    [InlineData("alice@example")]
    [InlineData("alice@@example.com")]
    [InlineData("alice example.com")]
    public void EmailRegex_DoesNotMatchInvalidEmails(string email)
    {
        Assert.False(ValidationRules.EmailRegex.IsMatch(email));
    }
}