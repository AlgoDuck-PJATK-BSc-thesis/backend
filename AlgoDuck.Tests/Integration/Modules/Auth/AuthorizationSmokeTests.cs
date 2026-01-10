using System.Net;
using System.Net.Http.Json;
using AlgoDuck.Tests.Integration.TestHost;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class AuthorizationSmokeTests
{
    readonly ApiCollectionFixture _fx;

    internal AuthorizationSmokeTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    static string? FindProtectedGetEndpoint(IServiceProvider sp)
    {
        var dataSources = sp.GetServices<EndpointDataSource>().ToList();

        foreach (var ds in dataSources)
        {
            foreach (var e in ds.Endpoints.OfType<RouteEndpoint>())
            {
                var http = e.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault();
                if (http is null || !http.HttpMethods.Contains("GET"))
                {
                    continue;
                }

                var hasAuth = e.Metadata.OfType<IAuthorizeData>().Any();
                if (!hasAuth)
                {
                    continue;
                }

                var raw = e.RoutePattern.RawText ?? "";
                if (raw.Contains("{"))
                {
                    continue;
                }

                if (!raw.StartsWith("/"))
                {
                    raw = "/" + raw;
                }

                return raw;
            }
        }

        return null;
    }

    static string BuildCookieHeader(IEnumerable<string> setCookieHeaders)
    {
        var parts = new List<string>();

        foreach (var sc in setCookieHeaders)
        {
            var first = sc.Split(';', 2, StringSplitOptions.TrimEntries)[0];
            if (!string.IsNullOrWhiteSpace(first))
            {
                parts.Add(first);
            }
        }

        return string.Join("; ", parts);
    }

    static async Task LoginAsAdminAsync(ApiCollectionFixture fx, HttpClient client)
    {
        var seed = new Seed(fx.Scope.ServiceProvider);
        var email = "admin@integration.test";
        var username = "admin_integration_test";
        var password = "Test1234!";

        await seed.CreateUserAsync(email, username, password, "Admin", true, CancellationToken.None);

        var payload = new Dictionary<string, object?>
        {
            ["userNameOrEmail"] = email,
            ["UserNameOrEmail"] = email,
            ["password"] = password,
            ["Password"] = password
        };

        var resp = await client.PostAsync("/api/auth/login", JsonContent.Create(payload), CancellationToken.None);

        if (resp.StatusCode != HttpStatusCode.OK)
        {
            var body = await resp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Login expected 200 OK but got {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
        }

        if (!resp.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            throw new Xunit.Sdk.XunitException("Login succeeded but no Set-Cookie header was returned.");
        }

        var cookieHeader = BuildCookieHeader(setCookies);

        client.DefaultRequestHeaders.Remove("Cookie");
        client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
    }

    [Fact]
    public async Task ProtectedEndpoint_Anonymous_IsUnauthorized()
    {
        var path = FindProtectedGetEndpoint(_fx.Scope.ServiceProvider);
        Assert.False(string.IsNullOrWhiteSpace(path));

        var client = _fx.CreateAnonymousClient();
        var resp = await client.GetAsync(path!, CancellationToken.None);

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_AfterLogin_IsNotUnauthorized()
    {
        var path = FindProtectedGetEndpoint(_fx.Scope.ServiceProvider);
        Assert.False(string.IsNullOrWhiteSpace(path));

        var client = _fx.CreateAnonymousClient();

        await LoginAsAdminAsync(_fx, client);

        var resp = await client.GetAsync(path!, CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.Unauthorized, resp.StatusCode);
    }
}
