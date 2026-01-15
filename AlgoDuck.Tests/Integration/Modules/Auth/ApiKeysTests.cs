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
    public async Task ApiKeys_GenerateListAndRevoke_ReturnsOk()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"apikeys_{suffix}@test.local";
        var username = $"apikeys_{suffix}";
        var password = "Test1234!";

        var client = _fx.CreateAnonymousClient();
        await AuthFlow.LoginAsync(_fx, client, email, username, password, "User", CancellationToken.None);
        SetCsrfHeaderFromCookie(client);

        var generateResp = await client.PostAsJsonAsync(
            "/api/auth/api-keys",
            new { Name = "integration-key" },
            CancellationToken.None);

        EnsureOkOrCreatedOrThrow(generateResp);

        var generateJson = await generateResp.Content.ReadAsStringAsync(CancellationToken.None);
        using var genDoc = JsonDocument.Parse(generateJson);

        var root = genDoc.RootElement;
        var body = root.TryGetProperty("body", out var b) ? b : root;

        if (!body.TryGetProperty("apiKey", out var apiKeyEl) || apiKeyEl.ValueKind != JsonValueKind.Object)
        {
            throw new Xunit.Sdk.XunitException($"Generate api key response did not contain apiKey object. Body: {generateJson}");
        }

        var idStr = TryGetString(apiKeyEl, "id") ?? TryGetString(apiKeyEl, "Id");
        if (string.IsNullOrWhiteSpace(idStr) || !Guid.TryParse(idStr, out var apiKeyId))
        {
            throw new Xunit.Sdk.XunitException($"Generate api key response did not contain a valid apiKey.id. Body: {generateJson}");
        }

        var listResp = await client.GetAsync("/api/auth/api-keys", CancellationToken.None);

        if (listResp.StatusCode != HttpStatusCode.OK)
        {
            var bodyTxt = await listResp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Expected 200 OK but got {(int)listResp.StatusCode} {listResp.StatusCode}. Body: {bodyTxt}");
        }

        var listJson = await listResp.Content.ReadAsStringAsync(CancellationToken.None);
        using var listDoc = JsonDocument.Parse(listJson);

        var listRoot = listDoc.RootElement;
        var listBody = listRoot.TryGetProperty("body", out var lb) ? lb : listRoot;

        Assert.True(listBody.ValueKind == JsonValueKind.Array);

        var found = false;
        foreach (var item in listBody.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object) continue;
            var itemId = TryGetString(item, "id") ?? TryGetString(item, "Id");
            if (string.Equals(itemId, apiKeyId.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                found = true;
                break;
            }
        }

        Assert.True(found);

        var revokeResp = await client.DeleteAsync($"/api/auth/api-keys/{apiKeyId}", CancellationToken.None);

        if (revokeResp.StatusCode != HttpStatusCode.OK && revokeResp.StatusCode != HttpStatusCode.NoContent)
        {
            var bodyTxt = await revokeResp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Expected 200 OK or 204 NoContent but got {(int)revokeResp.StatusCode} {revokeResp.StatusCode}. Body: {bodyTxt}");
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

    static void EnsureOkOrCreatedOrThrow(HttpResponseMessage resp)
    {
        if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Created)
        {
            return;
        }

        var body = resp.Content.ReadAsStringAsync(CancellationToken.None).GetAwaiter().GetResult();
        throw new Xunit.Sdk.XunitException($"Expected 200 OK or 201 Created but got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
    }

    static string? TryGetString(JsonElement obj, string prop)
    {
        if (!obj.TryGetProperty(prop, out var el)) return null;
        if (el.ValueKind != JsonValueKind.String) return null;
        return el.GetString();
    }
}
