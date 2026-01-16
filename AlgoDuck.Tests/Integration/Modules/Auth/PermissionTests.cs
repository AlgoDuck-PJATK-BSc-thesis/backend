using System.Net;
using System.Net.Http.Json;
using AlgoDuck.Tests.Integration.TestHost;
using Microsoft.Net.Http.Headers;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class PermissionsTests
{
    readonly ApiCollectionFixture _fx;

    public PermissionsTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task PermissionsCheck_WhenAuthenticated_ReturnsNotNotFoundOrMethodNotAllowed()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"perm_{suffix}@test.local";
        var username = $"perm_{suffix}";
        var password = "Test1234!";

        var client = _fx.CreateAnonymousClient();
        await AuthFlow.LoginAsync(_fx, client, email, username, password, "User", CancellationToken.None);
        SetCsrfHeaderFromCookie(client);

        var payload = new
        {
            Permissions = new[] { "user.read" }
        };

        var resp = await client.PostAsJsonAsync("/api/auth/permissions/check", payload, CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.NotEqual(HttpStatusCode.MethodNotAllowed, resp.StatusCode);

        if ((int)resp.StatusCode >= 500)
        {
            var bodyBad = await resp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Unexpected {(int)resp.StatusCode} {resp.StatusCode}. Body: {bodyBad}");
        }
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
