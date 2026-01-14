using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AlgoDuck.Tests.Integration.TestHost;
using Microsoft.Net.Http.Headers;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class ApiKeysTests
{
    readonly ApiCollectionFixture _fx;

    public ApiKeysTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task ApiKeys_GenerateAndRevoke_ReturnsOk()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];

        var email = $"apikey_{suffix}@test.local";
        var username = $"apikey_{suffix}";
        var password = "Test1234!";

        var client = _fx.CreateAnonymousClient();
        await AuthFlow.LoginAsync(_fx, client, email, username, password, "User", CancellationToken.None);
        SetCsrfHeaderFromCookie(client);

        var genResp = await client.PostAsJsonAsync(
            "/api/auth/api-keys",
            new { Name = "integration-key" },
            CancellationToken.None);

        EnsureOkOrThrow(genResp);

        var genJson = await genResp.Content.ReadAsStringAsync(CancellationToken.None);
        using var doc = JsonDocument.Parse(genJson);

        var root = doc.RootElement;

        JsonElement body;
        if (root.TryGetProperty("body", out var b))
        {
            body = b;
        }
        else
        {
            body = root;
        }

        if (!body.TryGetProperty("apiKey", out var apiKeyEl) || apiKeyEl.ValueKind != JsonValueKind.Object)
        {
            throw new Xunit.Sdk.XunitException($"Generate api key response did not contain body.apiKey object. Body: {genJson}");
        }

        var apiKeyId = TryGetGuid(apiKeyEl, "id") ?? TryGetGuid(apiKeyEl, "Id");
        if (!apiKeyId.HasValue || apiKeyId.Value == Guid.Empty)
        {
            throw new Xunit.Sdk.XunitException($"Generate api key response did not contain a valid id. Body: {genJson}");
        }

        var revokeResp = await client.DeleteAsync($"/api/auth/api-keys/{apiKeyId.Value}", CancellationToken.None);
        EnsureOkOrThrow(revokeResp);
    }

    [Fact]
    public async Task ApiKeys_Generate_WhenNameMissing_ReturnsError()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];

        var email = $"apikey_bad_{suffix}@test.local";
        var username = $"apikey_bad_{suffix}";
        var password = "Test1234!";

        var client = _fx.CreateAnonymousClient();
        await AuthFlow.LoginAsync(_fx, client, email, username, password, "User", CancellationToken.None);
        SetCsrfHeaderFromCookie(client);

        var genResp = await client.PostAsJsonAsync(
            "/api/auth/api-keys",
            new { Name = "" },
            CancellationToken.None);

        Assert.True((int)genResp.StatusCode >= 400);
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

    static Guid? TryGetGuid(JsonElement obj, string prop)
    {
        if (!obj.TryGetProperty(prop, out var el)) return null;

        if (el.ValueKind == JsonValueKind.String)
        {
            var s = el.GetString();
            if (Guid.TryParse(s, out var g)) return g;
            return null;
        }

        if (el.ValueKind == JsonValueKind.Null || el.ValueKind == JsonValueKind.Undefined) return null;

        try
        {
            return el.GetGuid();
        }
        catch
        {
            return null;
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
