using System.Net;
using System.Net.Http.Json;
using AlgoDuck.Tests.Integration.TestHost;
using Xunit;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class LoginTests
{
    readonly ApiCollectionFixture _fx;

    public LoginTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    static Task<HttpResponseMessage> LoginAsync(HttpClient client, string userNameOrEmail, string password, CancellationToken ct)
    {
        var payload = new Dictionary<string, object?>
        {
            ["userNameOrEmail"] = userNameOrEmail,
            ["UserNameOrEmail"] = userNameOrEmail,
            ["password"] = password,
            ["Password"] = password
        };

        return client.PostAsync("/api/auth/login", JsonContent.Create(payload), ct);
    }

    [Fact]
    public async Task Login_WhenValid_ReturnsOk()
    {
        var client = _fx.CreateAnonymousClient();

        var seed = new Seed(_fx.Scope.ServiceProvider);
        var email = "user@login.integration.test";
        var username = "user_login_integration_test";
        var password = "Test1234!";

        await seed.CreateUserAsync(email, username, password, "User", true, CancellationToken.None);

        var resp = await LoginAsync(client, email, password, CancellationToken.None);

        if (resp.StatusCode != HttpStatusCode.OK)
        {
            var body = await resp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Expected 200 OK but got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
        }

        Assert.True(resp.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.True(cookies.Any());
    }

    [Fact]
    public async Task Login_WhenInvalidPassword_ReturnsUnauthorizedOrBadRequest()
    {
        var client = _fx.CreateAnonymousClient();

        var seed = new Seed(_fx.Scope.ServiceProvider);
        var email = "user2@login.integration.test";
        var username = "user2_login_integration_test";
        var password = "Test1234!";

        await seed.CreateUserAsync(email, username, password, "User", true, CancellationToken.None);

        var resp = await LoginAsync(client, email, "WrongPassword!", CancellationToken.None);

        Assert.True(resp.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest);
    }
}
