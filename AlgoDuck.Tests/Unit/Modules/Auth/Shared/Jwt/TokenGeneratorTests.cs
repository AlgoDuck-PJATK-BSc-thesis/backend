using AlgoDuck.Modules.Auth.Shared.Jwt;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Jwt;

public sealed class TokenGeneratorTests
{
    [Fact]
    public void GenerateSecureToken_DefaultSize_IsBase64AndNotEmpty()
    {
        var generator = new TokenGenerator();

        var token = generator.GenerateSecureToken();

        Assert.False(string.IsNullOrWhiteSpace(token));

        var bytes = Convert.FromBase64String(token);
        Assert.Equal(32, bytes.Length);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    public void GenerateSecureToken_CustomSize_IsBase64AndExpectedLength(int size)
    {
        var generator = new TokenGenerator();

        var token = generator.GenerateSecureToken(size);

        Assert.False(string.IsNullOrWhiteSpace(token));

        var bytes = Convert.FromBase64String(token);
        Assert.Equal(size, bytes.Length);
    }

    [Fact]
    public void GenerateRefreshToken_IsBase64And32Bytes()
    {
        var generator = new TokenGenerator();

        var token = generator.GenerateRefreshToken();

        var bytes = Convert.FromBase64String(token);
        Assert.Equal(32, bytes.Length);
    }

    [Fact]
    public void GenerateCsrfToken_IsBase64And16Bytes()
    {
        var generator = new TokenGenerator();

        var token = generator.GenerateCsrfToken();

        var bytes = Convert.FromBase64String(token);
        Assert.Equal(16, bytes.Length);
    }

    [Fact]
    public void GenerateSecureToken_TwoCalls_AreDifferent()
    {
        var generator = new TokenGenerator();

        var a = generator.GenerateSecureToken();
        var b = generator.GenerateSecureToken();

        Assert.NotEqual(a, b);
    }
}