using AlgoDuck.Modules.Auth.Shared.Utils;
using Microsoft.AspNetCore.Http;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Utils;

public sealed class TokenUtilityTests
{
    [Fact]
    public void ValidateCsrf_WhenHeaderMissing_ReturnsFalse()
    {
        var utility = new TokenUtility();

        Assert.False(utility.ValidateCsrf("", "x"));
        Assert.False(utility.ValidateCsrf("   ", "x"));
    }

    [Fact]
    public void ValidateCsrf_WhenCookieMissing_ReturnsFalse()
    {
        var utility = new TokenUtility();

        Assert.False(utility.ValidateCsrf("x", null));
        Assert.False(utility.ValidateCsrf("x", ""));
        Assert.False(utility.ValidateCsrf("x", "   "));
    }

    [Fact]
    public void ValidateCsrf_WhenEqualOrdinal_ReturnsTrue()
    {
        var utility = new TokenUtility();

        Assert.True(utility.ValidateCsrf("token", "token"));
        Assert.False(utility.ValidateCsrf("Token", "token"));
    }

    [Fact]
    public void GetRefreshPrefix_WhenNullOrEmpty_ReturnsEmpty()
    {
        var utility = new TokenUtility();

        Assert.Equal(string.Empty, utility.GetRefreshPrefix(null!));
        Assert.Equal(string.Empty, utility.GetRefreshPrefix(""));
    }

    [Fact]
    public void GetRefreshPrefix_WhenShorterThan32_ReturnsWholeString()
    {
        var utility = new TokenUtility();
        var raw = "short_refresh";

        var prefix = utility.GetRefreshPrefix(raw);

        Assert.Equal(raw, prefix);
    }

    [Fact]
    public void GetRefreshPrefix_WhenLongerThan32_ReturnsFirst32Chars()
    {
        var utility = new TokenUtility();
        var raw = new string('a', 40);

        var prefix = utility.GetRefreshPrefix(raw);

        Assert.Equal(32, prefix.Length);
        Assert.Equal(raw.Substring(0, 32), prefix);
    }

    [Fact]
    public void GetSessionFromContext_ThrowsNotImplementedException()
    {
        var utility = new TokenUtility();
        var context = new DefaultHttpContext();

        var ex = Assert.Throws<InvalidOperationException>(() => utility.GetSessionFromContext(context));

        Assert.Equal("Session resolution from HttpContext is not implemented.", ex.Message);
    }
}
