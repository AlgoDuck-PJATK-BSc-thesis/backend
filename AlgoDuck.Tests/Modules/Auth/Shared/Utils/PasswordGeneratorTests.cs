using AlgoDuck.Modules.Auth.Shared.Utils;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Utils;

public sealed class PasswordGeneratorTests
{
    private const string AllowedCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*";

    [Fact]
    public void Generate_WhenLengthBelowMin_ClampsToMin()
    {
        var password = PasswordGenerator.Generate(1);

        Assert.Equal(ValidationRules.PasswordMinLength, password.Length);
    }

    [Fact]
    public void Generate_WhenLengthAboveMax_ClampsToMax()
    {
        var password = PasswordGenerator.Generate(10000);

        Assert.Equal(ValidationRules.PasswordMaxLength, password.Length);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(12)]
    [InlineData(64)]
    [InlineData(128)]
    public void Generate_WhenLengthInRange_UsesRequestedLength(int length)
    {
        var password = PasswordGenerator.Generate(length);

        Assert.Equal(length, password.Length);
    }

    [Fact]
    public void Generate_UsesOnlyAllowedCharacters()
    {
        var password = PasswordGenerator.Generate(64);

        Assert.False(string.IsNullOrWhiteSpace(password));
        Assert.All(password, c => Assert.Contains(c, AllowedCharacters));
    }
}