using AlgoDuck.Modules.Auth.Commands.ApiKeys.GenerateApiKey;

namespace AlgoDuck.Tests.Modules.Auth.Commands.GenerateApiKey;

public sealed class GenerateApiKeyValidatorTests
{
    [Fact]
    public void Validate_WhenNameEmpty_Fails()
    {
        var v = new GenerateApiKeyValidator();

        var r = v.Validate(new GenerateApiKeyDto
        {
            Name = "",
            LifetimeDays = null
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenNameTooLong_Fails()
    {
        var v = new GenerateApiKeyValidator();
        var name = new string('a', 257);

        var r = v.Validate(new GenerateApiKeyDto
        {
            Name = name,
            LifetimeDays = null
        });

        Assert.False(r.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WhenLifetimeDaysNotPositive_Fails(int days)
    {
        var v = new GenerateApiKeyValidator();

        var r = v.Validate(new GenerateApiKeyDto
        {
            Name = "key",
            LifetimeDays = days
        });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenLifetimeDaysNull_Passes()
    {
        var v = new GenerateApiKeyValidator();

        var r = v.Validate(new GenerateApiKeyDto
        {
            Name = "key",
            LifetimeDays = null
        });

        Assert.True(r.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_Passes()
    {
        var v = new GenerateApiKeyValidator();

        var r = v.Validate(new GenerateApiKeyDto
        {
            Name = "key",
            LifetimeDays = 7
        });

        Assert.True(r.IsValid);
    }
}