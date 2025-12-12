using AlgoDuck.Modules.Auth.Queries.GetApiKeys;

namespace AlgoDuck.Tests.Modules.Auth.Queries.GetApiKeys;

public sealed class GetApiKeysValidatorTests
{
    [Fact]
    public void Validate_WhenEmpty_Fails()
    {
        var validator = new GetApiKeysValidator();

        var result = validator.Validate(Guid.Empty);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenNonEmpty_Passes()
    {
        var validator = new GetApiKeysValidator();

        var result = validator.Validate(Guid.NewGuid());

        Assert.True(result.IsValid);
    }
}