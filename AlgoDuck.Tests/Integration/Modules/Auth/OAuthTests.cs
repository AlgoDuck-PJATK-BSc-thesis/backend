using System.Net;
using AlgoDuck.Tests.Integration.TestHost;

namespace AlgoDuck.Tests.Integration.Modules.Auth;

[Collection("Api")]
public sealed class OAuthTests
{
    readonly ApiCollectionFixture _fx;

    public OAuthTests(ApiCollectionFixture fx)
    {
        _fx = fx;
    }

    [Theory]
    [InlineData("google")]
    [InlineData("github")]
    public async Task OAuth_Start_ReturnsRedirectOrClientError(string provider)
    {
        var client = _fx.CreateAnonymousClient();

        var resp = await client.GetAsync($"/api/auth/oauth/{provider}/start", CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.NotEqual(HttpStatusCode.MethodNotAllowed, resp.StatusCode);

        if ((int)resp.StatusCode >= 500)
        {
            var bodyBad = await resp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Unexpected {(int)resp.StatusCode} {resp.StatusCode}. Body: {bodyBad}");
        }

        Assert.True(
            resp.StatusCode == HttpStatusCode.OK ||
            resp.StatusCode == HttpStatusCode.Redirect ||
            resp.StatusCode == HttpStatusCode.RedirectMethod ||
            resp.StatusCode == HttpStatusCode.RedirectKeepVerb ||
            resp.StatusCode == HttpStatusCode.Found ||
            (int)resp.StatusCode >= 400);
    }

    [Theory]
    [InlineData("google")]
    [InlineData("github")]
    public async Task OAuth_Complete_DoesNot500(string provider)
    {
        var client = _fx.CreateAnonymousClient();

        var resp = await client.GetAsync($"/api/auth/oauth/{provider}/complete", CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.NotEqual(HttpStatusCode.MethodNotAllowed, resp.StatusCode);

        if ((int)resp.StatusCode >= 500)
        {
            var bodyBad = await resp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Unexpected {(int)resp.StatusCode} {resp.StatusCode}. Body: {bodyBad}");
        }
    }

    [Theory]
    [InlineData("google")]
    [InlineData("github")]
    public async Task OAuth_Root_DoesNot500(string provider)
    {
        var client = _fx.CreateAnonymousClient();

        var resp = await client.GetAsync($"/api/auth/oauth/{provider}", CancellationToken.None);

        Assert.NotEqual(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.NotEqual(HttpStatusCode.MethodNotAllowed, resp.StatusCode);

        if ((int)resp.StatusCode >= 500)
        {
            var bodyBad = await resp.Content.ReadAsStringAsync(CancellationToken.None);
            throw new Xunit.Sdk.XunitException($"Unexpected {(int)resp.StatusCode} {resp.StatusCode}. Body: {bodyBad}");
        }
    }
}
