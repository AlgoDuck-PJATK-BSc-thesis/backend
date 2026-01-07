using AlgoDuck.Modules.Auth.Shared.Utils;
using AlgoDuck.Shared.Utilities;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Utils;

public sealed class ApiKeyGeneratorTests
{
    [Fact]
    public void Generate_ReturnsMaterialWithExpectedFields()
    {
        var material = ApiKeyGenerator.Generate();

        Assert.False(string.IsNullOrWhiteSpace(material.RawKey));
        Assert.False(string.IsNullOrWhiteSpace(material.Prefix));
        Assert.False(string.IsNullOrWhiteSpace(material.Hash));
        Assert.False(string.IsNullOrWhiteSpace(material.Salt));

        Assert.Equal(16, material.Prefix.Length);
        Assert.Equal(material.RawKey.Substring(0, 16), material.Prefix);
    }

    [Fact]
    public void Generate_HashMatchesRawKeyAndSalt()
    {
        var material = ApiKeyGenerator.Generate();

        var saltBytes = Convert.FromBase64String(material.Salt);
        var recomputedHash = HashingHelper.HashPassword(material.RawKey, saltBytes);

        Assert.Equal(material.Hash, recomputedHash);

        var hashBytes = Convert.FromBase64String(material.Hash);
        Assert.NotEmpty(hashBytes);
        Assert.NotEmpty(saltBytes);
    }
}