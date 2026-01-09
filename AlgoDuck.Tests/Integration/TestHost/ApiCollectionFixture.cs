using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace AlgoDuck.Tests.Integration.TestHost;

public sealed class ApiCollectionFixture : IAsyncLifetime
{
    public ApiFactory Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;
    public IServiceScope Scope { get; private set; } = null!;

    public Task InitializeAsync()
    {
        Factory = new ApiFactory();
        Client = Factory.CreateClient(new()
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });

        Scope = Factory.Services.CreateScope();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        Client.Dispose();

        Scope.Dispose();
        Factory.Dispose();
        return Task.CompletedTask;
    }

    public HttpClient CreateAnonymousClient()
    {
        var client = Factory.CreateClient(new()
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });
        client.DefaultRequestHeaders.Authorization = null;
        return client;
    }

    public HttpClient CreateAuthenticatedClient(string bearerToken)
    {
        var client = Factory.CreateClient(new()
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return client;
    }
}