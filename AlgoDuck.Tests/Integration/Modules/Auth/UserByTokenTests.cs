using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AlgoDuck.Tests.Integration.TestHost;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class UserByTokenTests
{
    readonly ApiCollectionFixture _fx;

    public UserByTokenTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task UserByToken_WithJwtFromLoginOrDummy_DoesNot500()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"userbytoken_{suffix}@test.local";
        var username = $"userbytoken_{suffix}";
        var password = "Test1234!";

        var client = _fx.CreateAnonymousClient();

        var loginPayload = new
        {
            UserNameOrEmail = username,
            Password = password,
            RememberMe = true
        };

        await AuthFlow.LoginAsync(_fx, client, email, username, password, "User", CancellationToken.None);

        var loginResp = await client.PostAsJsonAsync("/api/auth/login", loginPayload, CancellationToken.None);

        var token = await TryExtractJwtAsync(loginResp);
        token ??= "dummy-token";

        var payload = new
        {
            Token = token
        };

        var resp = await client.PostAsJsonAsync("/api/auth/user-by-token", payload, CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.NotEqual(HttpStatusCode.MethodNotAllowed, resp.StatusCode);

        if ((int)resp.StatusCode >= 500)
        {
            var bodyBad = await resp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Unexpected {(int)resp.StatusCode} {resp.StatusCode}. Body: {bodyBad}");
        }
    }

    static async Task<string?> TryExtractJwtAsync(HttpResponseMessage resp)
    {

        var json = await resp.Content.ReadAsStringAsync(CancellationToken.None);
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var body = root.TryGetProperty("body", out var b) ? b : root;

            var jwt = FindJwtString(body);
            return jwt;
        }
        catch
        {
            return null;
        }
    }

    static string? FindJwtString(JsonElement el)
    {
        if (el.ValueKind == JsonValueKind.String)
        {
            var s = el.GetString();
            if (LooksLikeJwt(s)) return s;
            return null;
        }

        if (el.ValueKind == JsonValueKind.Object)
        {
            foreach (var p in el.EnumerateObject())
            {
                var found = FindJwtString(p.Value);
                if (found is not null) return found;
            }
        }

        if (el.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in el.EnumerateArray())
            {
                var found = FindJwtString(item);
                if (found is not null) return found;
            }
        }

        return null;
    }

    static bool LooksLikeJwt(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        var parts = s.Split('.');
        if (parts.Length != 3) return false;
        if (parts[0].Length < 5 || parts[1].Length < 5 || parts[2].Length < 5) return false;
        return true;
    }
}
