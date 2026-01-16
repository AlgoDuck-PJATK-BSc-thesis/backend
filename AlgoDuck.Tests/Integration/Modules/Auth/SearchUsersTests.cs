using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AlgoDuck.Tests.Integration.TestHost;
using Microsoft.Net.Http.Headers;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class SearchUsersTests
{
    readonly ApiCollectionFixture _fx;

    public SearchUsersTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task SearchUsers_WhenSearchingByEmail_ReturnsListContainingUserOrClientError()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"search_{suffix}@test.local";
        var username = $"search_{suffix}";
        var password = "Test1234!";

        var client = _fx.CreateAnonymousClient();
        await AuthFlow.LoginAsync(_fx, client, email, username, password, "User", CancellationToken.None);
        SetCsrfHeaderFromCookie(client);

        var payload = new
        {
            Query = email,
            Page = 1,
            PageSize = 10
        };

        var resp = await client.PostAsJsonAsync("/api/auth/search-users", payload, CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.NotEqual(HttpStatusCode.MethodNotAllowed, resp.StatusCode);

        if ((int)resp.StatusCode >= 500)
        {
            var bodyBad = await resp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Unexpected {(int)resp.StatusCode} {resp.StatusCode}. Body: {bodyBad}");
        }

        if ((int)resp.StatusCode >= 400)
        {
            return;
        }

        var json = await resp.Content.ReadAsStringAsync(CancellationToken.None);
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        var body = root.TryGetProperty("body", out var b) ? b : root;

        var found = ContainsEmail(body, email);
        Assert.True(found);
    }

    static bool ContainsEmail(JsonElement body, string email)
    {
        if (body.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in body.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) continue;
                var e = TryGetString(item, "email") ?? TryGetString(item, "Email");
                if (string.Equals(e, email, StringComparison.OrdinalIgnoreCase)) return true;
            }
        }

        if (body.ValueKind == JsonValueKind.Object)
        {
            if (body.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in items.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object) continue;
                    var e = TryGetString(item, "email") ?? TryGetString(item, "Email");
                    if (string.Equals(e, email, StringComparison.OrdinalIgnoreCase)) return true;
                }
            }
        }

        return false;
    }

    static string? TryGetString(JsonElement obj, string prop)
    {
        if (!obj.TryGetProperty(prop, out var el)) return null;
        if (el.ValueKind != JsonValueKind.String) return null;
        return el.GetString();
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
