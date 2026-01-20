using System.Net;
using System.Text.Json;
using AlgoDuck.Tests.Integration.TestHost;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class MeTests
{
    readonly ApiCollectionFixture _fx;

    public MeTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task Me_WhenAuthenticated_ReturnsUserPayload()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"me_{suffix}@test.local";
        var username = $"me_{suffix}";
        var password = "Test1234!";

        var client = _fx.CreateAnonymousClient();
        await AuthFlow.LoginAsync(_fx, client, email, username, password, "User", CancellationToken.None);

        var resp = await client.GetAsync("/api/auth/me", CancellationToken.None);

        if (resp.StatusCode != HttpStatusCode.OK)
        {
            var bodyBad = await resp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Expected 200 OK but got {(int)resp.StatusCode} {resp.StatusCode}. Body: {bodyBad}");
        }

        var json = await resp.Content.ReadAsStringAsync(CancellationToken.None);
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        var body = root.TryGetProperty("body", out var b) ? b : root;

        var gotUserName = TryGetString(body, "userName") ?? TryGetString(body, "UserName") ?? "";
        var gotEmail = TryGetString(body, "email") ?? TryGetString(body, "Email") ?? "";

        Assert.Equal(username, gotUserName);
        Assert.Equal(email, gotEmail);
    }

    static string? TryGetString(JsonElement obj, string prop)
    {
        if (!obj.TryGetProperty(prop, out var el)) return null;
        if (el.ValueKind != JsonValueKind.String) return null;
        return el.GetString();
    }
}