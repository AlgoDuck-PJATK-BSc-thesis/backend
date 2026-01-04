using AlgoDuck.Modules.Auth.Queries.Identity.GetCurrentUser;

namespace AlgoDuck.Tests.Modules.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserValidatorTests
{
    [Fact]
    public void Validate_WhenEmpty_Fails()
    {
        var validator = new GetCurrentUserValidator();

        var result = validator.Validate(Guid.Empty);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenNonEmpty_Passes()
    {
        var validator = new GetCurrentUserValidator();

        var result = validator.Validate(Guid.NewGuid());

        Assert.True(result.IsValid);
    }
}