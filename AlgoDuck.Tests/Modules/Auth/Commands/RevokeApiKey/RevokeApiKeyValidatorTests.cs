using AlgoDuck.Modules.Auth.Commands.RevokeApiKey;

namespace AlgoDuck.Tests.Modules.Auth.Commands.RevokeApiKey;

public sealed class RevokeApiKeyValidatorTests
{
    [Fact]
    public void Validate_AlwaysPasses()
    {
        var v = new RevokeApiKeyValidator();

        var r = v.Validate(new RevokeApiKeyDto());

        Assert.True(r.IsValid);
    }
}