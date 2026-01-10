using System.Net;
using System.Text.Json;
using AlgoDuck.Tests.Integration.TestHost;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class RefreshTokenTests
{
    readonly ApiCollectionFixture _fx;

    internal RefreshTokenTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task Refresh_WhenCookieMissing_ReturnsUnauthorized_WithMessage()
    {
        var client = _fx.CreateAnonymousClient();

        var resp = await client.PostAsync("/api/auth/refresh", content: null, CancellationToken.None);

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);

        var json = await resp.Content.ReadAsStringAsync(CancellationToken.None);
        using var doc = JsonDocument.Parse(json);

        var msg = doc.RootElement.TryGetProperty("message", out var m) ? m.GetString() : null;
        Assert.Equal("Refresh token cookie is missing.", msg);
    }

    [Fact]
    public async Task Refresh_WhenCookiePresent_WithCsrfHeader_ReturnsOk_AndSetsCookies()
    {
        var client = _fx.CreateAnonymousClient();

        var email = "refresh_ok@test.local";
        var username = "refresh_ok";
        var password = "Test1234!";

        await AuthFlow.LoginAsync(_fx, client, email, username, password, "User", CancellationToken.None);

        var csrf = ExtractCookieValueFromClient(client, "csrf_token");
        Assert.False(string.IsNullOrWhiteSpace(csrf));

        client.DefaultRequestHeaders.Remove("X-CSRF-Token");
        client.DefaultRequestHeaders.Add("X-CSRF-Token", csrf!);

        var resp = await client.PostAsync("/api/auth/refresh", content: null, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        Assert.True(resp.Headers.TryGetValues(HeaderNames.SetCookie, out var setCookies));
        Assert.True(setCookies.ToList().Count >= 1);

        var json = await resp.Content.ReadAsStringAsync(CancellationToken.None);
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("body", out var body));
        Assert.Equal("Tokens refreshed successfully.", body.GetProperty("message").GetString());

        var userIdEl = body.GetProperty("userId");
        if (userIdEl.ValueKind == JsonValueKind.String)
            Assert.True(Guid.TryParse(userIdEl.GetString(), out _));
        else
            Assert.True(Guid.TryParse(userIdEl.GetGuid().ToString(), out _));

        var sessionIdEl = body.GetProperty("sessionId");
        if (sessionIdEl.ValueKind == JsonValueKind.String)
            Assert.True(Guid.TryParse(sessionIdEl.GetString(), out _));
        else
            Assert.True(Guid.TryParse(sessionIdEl.GetGuid().ToString(), out _));

        Assert.True(body.TryGetProperty("accessTokenExpiresAt", out var _));
        Assert.True(body.TryGetProperty("refreshTokenExpiresAt", out var _));
    }

    [Fact]
    public async Task Refresh_WhenCookiePresent_ButCsrfHeaderMissing_ReturnsForbidden()
    {
        var client = _fx.CreateAnonymousClient();

        var email = "refresh_forbidden@test.local";
        var username = "refresh_forbidden";
        var password = "Test1234!";

        await AuthFlow.LoginAsync(_fx, client, email, username, password, "User", CancellationToken.None);

        client.DefaultRequestHeaders.Remove("X-CSRF-Token");

        var resp = await client.PostAsync("/api/auth/refresh", content: null, CancellationToken.None);

        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    static string? ExtractCookieValueFromClient(HttpClient client, string cookieName)
    {
        if (!client.DefaultRequestHeaders.TryGetValues(HeaderNames.Cookie, out var values))
        {
            return null;
        }

        var header = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(header))
        {
            return null;
        }

        foreach (var part in header.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var idx = part.IndexOf('=', StringComparison.Ordinal);
            if (idx <= 0) continue;

            var name = part[..idx].Trim();
            if (!name.Equals(cookieName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var val = part[(idx + 1)..].Trim();
            return string.IsNullOrWhiteSpace(val) ? null : val;
        }

        return null;
    }
}
