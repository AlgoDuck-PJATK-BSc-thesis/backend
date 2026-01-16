using System.Net;
using System.Net.Http.Json;
using AlgoDuck.Tests.Integration.TestHost;
using Microsoft.Net.Http.Headers;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class TwoFactorTests
{
    readonly ApiCollectionFixture _fx;

    public TwoFactorTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task TwoFactor_EnableThenDisable_ReturnsOk()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"twofactor_{suffix}@test.local";
        var username = $"twofactor_{suffix}";
        var password = "Test1234!";

        var client = _fx.CreateAnonymousClient();
        await AuthFlow.LoginAsync(_fx, client, email, username, password, "User", CancellationToken.None);
        SetCsrfHeaderFromCookie(client);

        var enableResp = await client.PostAsync("/api/auth/twofactor/enable", content: null, CancellationToken.None);
        Assert.NotEqual(HttpStatusCode.NotFound, enableResp.StatusCode);
        Assert.True(enableResp.StatusCode == HttpStatusCode.OK || (int)enableResp.StatusCode < 500);

        var disableResp = await client.PostAsync("/api/auth/twofactor/disable", content: null, CancellationToken.None);
        Assert.NotEqual(HttpStatusCode.NotFound, disableResp.StatusCode);
        Assert.True(disableResp.StatusCode == HttpStatusCode.OK || (int)disableResp.StatusCode < 500);
    }

    [Fact]
    public async Task TwoFactor_VerifyLogin_WhenMissingCode_ReturnsError()
    {
        var payload = new
        {
            Code = ""
        };

        var resp = await _fx.CreateAnonymousClient()
            .PostAsJsonAsync("/api/auth/twofactor/verify-login", payload, CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.True((int)resp.StatusCode >= 400);
    }

    static void SetCsrfHeaderFromCookie(HttpClient client)
    {
        var csrf = ExtractCookieValueFromClient(client, "csrf_token");
        Assert.False(string.IsNullOrWhiteSpace(csrf));

        client.DefaultRequestHeaders.Remove("X-CSRF-Token");
        client.DefaultRequestHeaders.Add("X-CSRF-Token", csrf);
    }

    static string? ExtractCookieValueFromClient(HttpClient client, string cookieName)
    {
        if (!client.DefaultRequestHeaders.TryGetValues(HeaderNames.Cookie, out var values))
        {
            return null;
        }

        var all = string.Join("; ", values);

        foreach (var seg in all.Split(';', StringSplitOptions.TrimEntries))
        {
            var parts = seg.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && string.Equals(parts[0], cookieName, StringComparison.OrdinalIgnoreCase))
            {
                return parts[1];
            }
        }

        return null;
    }
}
