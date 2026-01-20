using System.Net;
using AlgoDuck.Tests.Integration.TestHost;
using Microsoft.Net.Http.Headers;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class LogoutTests
{
    readonly ApiCollectionFixture _fx;

    public LogoutTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task Logout_WhenAuthenticated_ReturnsOk_AndRefreshFails()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"logout_{suffix}@test.local";
        var username = $"logout_{suffix}";
        var password = "Test1234!";

        var client = _fx.CreateAnonymousClient();
        await AuthFlow.LoginAsync(_fx, client, email, username, password, "User", CancellationToken.None);
        SetCsrfHeaderFromCookie(client);

        var logoutResp = await client.PostAsync("/api/auth/logout", content: null, CancellationToken.None);

        EnsureOkOrThrow(logoutResp);

        var refreshResp = await client.PostAsync("/api/auth/refresh", content: null, CancellationToken.None);

        Assert.True(refreshResp.StatusCode == HttpStatusCode.Unauthorized || refreshResp.StatusCode == HttpStatusCode.Forbidden);
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

    static void EnsureOkOrThrow(HttpResponseMessage resp)
    {
        if (resp.StatusCode == HttpStatusCode.OK)
        {
            return;
        }

        var body = resp.Content.ReadAsStringAsync(CancellationToken.None).GetAwaiter().GetResult();
        throw new Xunit.Sdk.XunitException($"Expected 200 OK but got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
    }
}
