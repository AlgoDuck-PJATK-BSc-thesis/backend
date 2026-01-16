using AlgoDuck.DAL;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace AlgoDuck.Tests.Integration.TestHost;

internal sealed class ApiCollectionFixture : IAsyncLifetime
{
    private ApiFactory _factory = null!;

    public HttpClient Client { get; private set; } = null!;
    public IServiceScope Scope { get; private set; } = null!;

    public Task InitializeAsync()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("ASPNETCORE_DETAILEDERRORS", "true");

        Environment.SetEnvironmentVariable("SEED_ADMIN_PASSWORD", "Test123!@#");
        Environment.SetEnvironmentVariable("SEED_USER_PASSWORD", "Test123!@#");

        _factory = new ApiFactory();

        Client = _factory.CreateClient(new()
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });

        Scope = _factory.Services.CreateScope();

        var cmd = Scope.ServiceProvider.GetRequiredService<ApplicationCommandDbContext>();
        var qry = Scope.ServiceProvider.GetRequiredService<ApplicationQueryDbContext>();

        cmd.Database.EnsureCreated();
        qry.Database.EnsureCreated();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        try
        {
            Client.DefaultRequestHeaders.Authorization = null;
            Client.Dispose();
        }
        finally
        {
            Scope.Dispose();
            _factory.Dispose();
        }

        return Task.CompletedTask;
    }

    public HttpClient CreateAnonymousClient()
    {
        var client = _factory.CreateClient(new()
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });

        client.DefaultRequestHeaders.Authorization = null;
        return client;
    }

    public HttpClient CreateAuthenticatedClient(string bearerToken)
    {
        var client = _factory.CreateClient(new()
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return client;
    }
}
