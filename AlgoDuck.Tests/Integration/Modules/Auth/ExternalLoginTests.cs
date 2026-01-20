using System.Net;
using System.Net.Http.Json;
using AlgoDuck.Tests.Integration.TestHost;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class ExternalLoginTests
{
    readonly ApiCollectionFixture _fx;

    public ExternalLoginTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task ExternalLogin_WhenProviderMissing_ReturnsError()
    {
        var payload = new
        {
            Provider = "",
            ReturnUrl = "http://localhost"
        };

        var resp = await _fx.CreateAnonymousClient()
            .PostAsJsonAsync("/api/auth/external-login", payload, CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.True((int)resp.StatusCode >= 400);
    }

    [Fact]
    public async Task ExternalLogin_WhenUnknownProvider_ReturnsError()
    {
        var payload = new
        {
            Provider = "unknown-provider",
            ReturnUrl = "http://localhost"
        };

        var resp = await _fx.CreateAnonymousClient()
            .PostAsJsonAsync("/api/auth/external-login", payload, CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.True((int)resp.StatusCode >= 400 || resp.StatusCode == HttpStatusCode.NotImplemented);
    }
}