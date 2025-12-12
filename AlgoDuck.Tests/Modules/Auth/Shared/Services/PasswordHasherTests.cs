using AlgoDuck.Modules.Auth.Shared.Services;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Services;

public sealed class PasswordHasherTests
{
    [Fact]
    public void HashPassword_WhenPasswordIsEmpty_ThenThrows()
    {
        var hasher = new PasswordHasher();

        Assert.Throws<ArgumentException>(() => hasher.HashPassword(""));
        Assert.Throws<ArgumentException>(() => hasher.HashPassword("   "));
    }

    [Fact]
    public void HashPassword_WhenPasswordIsValid_ThenReturnsSaltDotHash()
    {
        var hasher = new PasswordHasher();

        var hashed = hasher.HashPassword("P@ssw0rd!");

        Assert.False(string.IsNullOrWhiteSpace(hashed));
        var parts = hashed.Split('.', 2);
        Assert.Equal(2, parts.Length);
        Assert.False(string.IsNullOrWhiteSpace(parts[0]));
        Assert.False(string.IsNullOrWhiteSpace(parts[1]));

        var saltBytes = Convert.FromBase64String(parts[0]);
        var hashBytes = Convert.FromBase64String(parts[1]);
        Assert.NotEmpty(saltBytes);
        Assert.NotEmpty(hashBytes);
    }

    [Fact]
    public void VerifyHashedPassword_WhenHashedPasswordIsEmpty_ThenReturnsFalse()
    {
        var hasher = new PasswordHasher();

        Assert.False(hasher.VerifyHashedPassword("", "x"));
        Assert.False(hasher.VerifyHashedPassword("   ", "x"));
    }

    [Fact]
    public void VerifyHashedPassword_WhenProvidedPasswordIsEmpty_ThenReturnsFalse()
    {
        var hasher = new PasswordHasher();

        Assert.False(hasher.VerifyHashedPassword("a.b", ""));
        Assert.False(hasher.VerifyHashedPassword("a.b", "   "));
    }

    [Fact]
    public void VerifyHashedPassword_WhenFormatIsInvalid_ThenReturnsFalse()
    {
        var hasher = new PasswordHasher();

        Assert.False(hasher.VerifyHashedPassword("no_dot_separator", "x"));
        Assert.False(hasher.VerifyHashedPassword(".", "x"));
        Assert.False(hasher.VerifyHashedPassword("a.", "x"));
        Assert.False(hasher.VerifyHashedPassword(".b", "x"));
    }

    [Fact]
    public void VerifyHashedPassword_WhenBase64IsInvalid_ThenReturnsFalse()
    {
        var hasher = new PasswordHasher();

        Assert.False(hasher.VerifyHashedPassword("not_base64.not_base64", "x"));
        Assert.False(hasher.VerifyHashedPassword("bm90YmFzZTY0.not_base64", "x"));
        Assert.False(hasher.VerifyHashedPassword("not_base64.bm90YmFzZTY0", "x"));
    }

    [Fact]
    public void VerifyHashedPassword_WhenPasswordMatches_ThenReturnsTrue()
    {
        var hasher = new PasswordHasher();

        var password = "P@ssw0rd!";
        var hashed = hasher.HashPassword(password);

        Assert.True(hasher.VerifyHashedPassword(hashed, password));
    }

    [Fact]
    public void VerifyHashedPassword_WhenPasswordDoesNotMatch_ThenReturnsFalse()
    {
        var hasher = new PasswordHasher();

        var hashed = hasher.HashPassword("P@ssw0rd!");

        Assert.False(hasher.VerifyHashedPassword(hashed, "different"));
    }
}
