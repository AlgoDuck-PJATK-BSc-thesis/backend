using AlgoDuck.Modules.Auth.Queries.Identity.SearchUsersByEmail;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Queries.Identity.SearchUsersByEmail;

public sealed class SearchUsersByEmailValidatorTests
{
    [Fact]
    public void Validate_WhenQueryEmpty_Fails()
    {
        var validator = new SearchUsersByEmailValidator();

        var result = validator.Validate(new SearchUsersByEmailDto { Query = "", Limit = 20 });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenQueryTooShort_Fails()
    {
        var validator = new SearchUsersByEmailValidator();

        var result = validator.Validate(new SearchUsersByEmailDto { Query = "a", Limit = 20 });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenQueryTooLong_Fails()
    {
        var validator = new SearchUsersByEmailValidator();
        var q = new string('a', 257);

        var result = validator.Validate(new SearchUsersByEmailDto { Query = q, Limit = 20 });

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_WhenLimitOutOfRange_Fails(int limit)
    {
        var validator = new SearchUsersByEmailValidator();

        var result = validator.Validate(new SearchUsersByEmailDto { Query = "ab", Limit = limit });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_Passes()
    {
        var validator = new SearchUsersByEmailValidator();

        var result = validator.Validate(new SearchUsersByEmailDto { Query = "ab", Limit = 20 });

        Assert.True(result.IsValid);
    }
}