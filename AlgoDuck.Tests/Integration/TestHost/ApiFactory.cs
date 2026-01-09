using AlgoDuck.DAL;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using StackExchange.Redis;

namespace AlgoDuck.Tests.Integration.TestHost;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    readonly string _sqliteFilePath;

    public ApiFactory()
    {
        _sqliteFilePath = Path.Combine(Path.GetTempPath(), $"algoduck_tests_{Guid.NewGuid():N}.sqlite");

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", $"Data Source={_sqliteFilePath}");
        Environment.SetEnvironmentVariable("ConnectionStrings__Redis", "localhost:6379");

        Environment.SetEnvironmentVariable("EMAIL__PROVIDER", "postmark");
        Environment.SetEnvironmentVariable("EMAIL__POSTMARKAPIKEY", "test_postmark_key");
        Environment.SetEnvironmentVariable("EMAIL__FROM", "noreply@test.local");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_sqliteFilePath}",
                ["ConnectionStrings:Redis"] = "localhost:6379",
                ["Email:Provider"] = "postmark",
                ["Email:PostmarkApiKey"] = "test_postmark_key",
                ["Email:From"] = "noreply@test.local"
            };

            config.AddInMemoryCollection(overrides);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IHostedService));
            services.AddHostedService<EmptyCohortCleanupService>();

            services.RemoveAll(typeof(IConnectionMultiplexer));
            services.RemoveAll(typeof(IDatabase));

            var dbMock = new Mock<IDatabase>(MockBehavior.Loose);
            var muxMock = new Mock<IConnectionMultiplexer>(MockBehavior.Loose);
            muxMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);

            services.AddSingleton<IConnectionMultiplexer>(muxMock.Object);
            services.AddSingleton<IDatabase>(dbMock.Object);

            services.RemoveAll(typeof(IEmailTransport));
            services.AddScoped<IEmailTransport, FakeEmailTransport>();

            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var cmd = scope.ServiceProvider.GetRequiredService<ApplicationCommandDbContext>();
            var qry = scope.ServiceProvider.GetRequiredService<ApplicationQueryDbContext>();

            cmd.Database.EnsureCreated();
            qry.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            try
            {
                if (File.Exists(_sqliteFilePath))
                {
                    File.Delete(_sqliteFilePath);
                }
            }
            catch
            {
            }
        }
    }
}
