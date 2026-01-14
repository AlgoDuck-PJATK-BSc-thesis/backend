using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AlgoDuck.Tests.Integration.TestHost;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class RegisterTests
{
    readonly ApiCollectionFixture _fx;

    public RegisterTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task Register_WhenValid_ReturnsCreated_AndUserPayload()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"register_{suffix}@test.local";
        var username = $"register_{suffix}";
        var password = "Test1234!";

        var client = _fx.CreateAnonymousClient();

        var payload = new
        {
            UserName = username,
            Email = email,
            Password = password,
            ConfirmPassword = password
        };

        var resp = await client.PostAsJsonAsync("/api/auth/register", payload, CancellationToken.None);

        EnsureOkOrCreatedOrThrow(resp);

        var json = await resp.Content.ReadAsStringAsync(CancellationToken.None);
        using var doc = JsonDocument.Parse(json);

        var body = doc.RootElement.TryGetProperty("body", out var b) ? b : doc.RootElement;

        var rUserName = TryGetString(body, "userName") ?? TryGetString(body, "UserName") ?? "";
        var rEmail = TryGetString(body, "email") ?? TryGetString(body, "Email") ?? "";

        Assert.Equal(username, rUserName);
        Assert.Equal(email, rEmail);
    }

    [Fact]
    public async Task Register_WhenDuplicateEmail_ReturnsError()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var email = $"register_dup_{suffix}@test.local";
        var usernameA = $"register_dup_a_{suffix}";
        var usernameB = $"register_dup_b_{suffix}";
        var password = "Test1234!";

        var client = _fx.CreateAnonymousClient();

        var payloadA = new
        {
            UserName = usernameA,
            Email = email,
            Password = password,
            ConfirmPassword = password
        };

        var first = await client.PostAsJsonAsync("/api/auth/register", payloadA, CancellationToken.None);
        EnsureOkOrCreatedOrThrow(first);

        var payloadB = new
        {
            UserName = usernameB,
            Email = email,
            Password = password,
            ConfirmPassword = password
        };

        var second = await client.PostAsJsonAsync("/api/auth/register", payloadB, CancellationToken.None);

        Assert.True((int)second.StatusCode >= 400);
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
