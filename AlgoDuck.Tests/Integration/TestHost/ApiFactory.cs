using System.Text.Encodings.Web;
using AlgoDuck.DAL;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;

namespace AlgoDuck.Tests.Integration.TestHost;

internal sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _sqliteFilePath;

    public ApiFactory()
    {
        _sqliteFilePath = Path.Combine(Path.GetTempPath(), $"algoduck_tests_{Guid.NewGuid():N}.sqlite");

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", $"Data Source={_sqliteFilePath}");
        Environment.SetEnvironmentVariable("ConnectionStrings__Redis", "localhost:6379");

        Environment.SetEnvironmentVariable("EMAIL__PROVIDER", "postmark");
        Environment.SetEnvironmentVariable("EMAIL__POSTMARKAPIKEY", "test_postmark_key");
        Environment.SetEnvironmentVariable("EMAIL__FROM", "noreply@test.local");

        Environment.SetEnvironmentVariable("APP__FRONTENDURL", "http://localhost:5173");
        Environment.SetEnvironmentVariable("APP__PUBLICAPIURL", "http://localhost:8080");
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
                ["Email:From"] = "noreply@test.local",
                ["App:FrontendUrl"] = "http://localhost:5173",
                ["App:PublicApiUrl"] = "http://localhost:8080",
                ["CORS:DevOrigins:0"] = "http://localhost:5173"
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

            dbMock.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            dbMock.Setup(x => x.StringSetAsync(
                    It.IsAny<RedisKey>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<When>(),
                    It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            dbMock.Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            dbMock.Setup(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            dbMock.Setup(x => x.SetRemoveAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            dbMock.Setup(x => x.SetMembersAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(Array.Empty<RedisValue>());

            dbMock.Setup(x => x.HashGetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            dbMock.Setup(x => x.HashSetAsync(It.IsAny<RedisKey>(), It.IsAny<HashEntry[]>(), It.IsAny<CommandFlags>()))
                .Returns(Task.CompletedTask);

            var muxMock = new Mock<IConnectionMultiplexer>(MockBehavior.Loose);
            muxMock.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(dbMock.Object);

            services.AddSingleton(muxMock.Object);
            services.AddSingleton(dbMock.Object);

            services.RemoveAll(typeof(IEmailTransport));
            services.RemoveAll(typeof(FakeEmailTransport));
            services.AddSingleton<FakeEmailTransport>();
            services.AddSingleton<IEmailTransport>(sp => sp.GetRequiredService<FakeEmailTransport>());

            services.RemoveAll(typeof(ApplicationCommandDbContext));
            services.RemoveAll(typeof(ApplicationQueryDbContext));
            services.RemoveAll(typeof(DbContextOptions<ApplicationCommandDbContext>));
            services.RemoveAll(typeof(DbContextOptions<ApplicationQueryDbContext>));

            var npgsqlDescriptors = services
                .Where(d =>
                    (d.ServiceType.FullName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.ImplementationType?.FullName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.ServiceType.FullName?.Contains("EntityFrameworkCore.PostgreSQL", StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.ImplementationType?.FullName?.Contains("EntityFrameworkCore.PostgreSQL", StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

            foreach (var d in npgsqlDescriptors)
            {
                services.Remove(d);
            }

            var sqliteConn = $"Data Source={_sqliteFilePath}";

            services.AddDbContext<ApplicationCommandDbContext>(o => o.UseSqlite(sqliteConn));
            services.AddDbContext<ApplicationQueryDbContext>(o => o.UseSqlite(sqliteConn));

            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestExternalAuthHandler>("Google", _ => { })
                .AddScheme<AuthenticationSchemeOptions, TestExternalAuthHandler>("GitHub", _ => { });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing) return;

        if (File.Exists(_sqliteFilePath))
        {
            File.Delete(_sqliteFilePath);
        }
    }

    private sealed class TestExternalAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestExternalAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status302Found;
            Response.Headers.Location = "http://external.test/";
            return Task.CompletedTask;
        }
    }
}
