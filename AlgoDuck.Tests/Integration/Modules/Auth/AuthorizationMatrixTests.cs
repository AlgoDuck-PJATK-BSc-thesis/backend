using System.Net;
using AlgoDuck.Tests.Integration.TestHost;
using Xunit;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class AuthorizationMatrixTests
{
    readonly ApiCollectionFixture _fx;

    public AuthorizationMatrixTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    static bool IsSuccess(HttpStatusCode code)
    {
        var n = (int)code;
        return n >= 200 && n <= 299;
    }

    [Fact]
    public async Task AllProtectedEndpoints_Anonymous_DoesNotSucceed()
    {
        var eps = EndpointCatalog.GetApiEndpoints(_fx.Scope.ServiceProvider)
            .Where(e => e.RequiresAuth)
            .ToList();

        var client = _fx.CreateAnonymousClient();

        foreach (var ep in eps)
        {
            foreach (var method in ep.Methods)
            {
                var req = EndpointCatalog.CreateRequest(method, ep.Route);
                var resp = await client.SendAsync(req, CancellationToken.None);

                if (IsSuccess(resp.StatusCode))
                {
                    var body = await resp.Content.ReadAsStringAsync(CancellationToken.None);
                    throw new Xunit.Sdk.XunitException($"Anonymous request unexpectedly succeeded: {method} {ep.Route} -> {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
                }
            }
        }
    }

    [Fact]
    public async Task AdminOnlyEndpoints_User_DoesNotSucceed()
    {
        var eps = EndpointCatalog.GetApiEndpoints(_fx.Scope.ServiceProvider)
            .Where(e => e.RequiresAuth && e.IsAdminOnly)
            .ToList();

        var client = _fx.CreateAnonymousClient();
        await AuthFlow.LoginAsync(_fx, client, "user_auth_matrix@test.local", "user_auth_matrix", "Test1234!", "User", CancellationToken.None);

        foreach (var ep in eps)
        {
            foreach (var method in ep.Methods)
            {
                var req = EndpointCatalog.CreateRequest(method, ep.Route);
                var resp = await client.SendAsync(req, CancellationToken.None);

                if (IsSuccess(resp.StatusCode))
                {
                    var body = await resp.Content.ReadAsStringAsync(CancellationToken.None);
                    throw new Xunit.Sdk.XunitException($"User request unexpectedly succeeded on admin-only endpoint: {method} {ep.Route} -> {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
                }
            }
        }
    }

    [Fact]
    public async Task ProtectedEndpoints_Admin_IsNotUnauthorized()
    {
        var eps = EndpointCatalog.GetApiEndpoints(_fx.Scope.ServiceProvider)
            .Where(e => e.RequiresAuth)
            .ToList();

        var client = _fx.CreateAnonymousClient();
        await AuthFlow.LoginAsync(_fx, client, "admin_auth_matrix@test.local", "admin_auth_matrix", "Test1234!", "Admin", CancellationToken.None);

        foreach (var ep in eps)
        {
            foreach (var method in ep.Methods)
            {
                var req = EndpointCatalog.CreateRequest(method, ep.Route);
                var resp = await client.SendAsync(req, CancellationToken.None);

                Assert.NotEqual(HttpStatusCode.Unauthorized, resp.StatusCode);
            }
        }
    }
}
