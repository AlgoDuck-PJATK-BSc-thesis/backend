using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AlgoDuck.DAL;

public sealed class ApplicationCommandDbContextFactory : IDesignTimeDbContextFactory<ApplicationCommandDbContext>
{
    public ApplicationCommandDbContext CreateDbContext(string[] args)
    {
        var baseDir = Directory.GetCurrentDirectory();
        var settingsDir = Path.Combine(baseDir, "AlgoDuck");
        if (!Directory.Exists(settingsDir))
        {
            settingsDir = baseDir;
        }

        var config = new ConfigurationBuilder()
            .SetBasePath(settingsDir)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            config.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? BuildFromPostgresEnv();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection (set ConnectionStrings__DefaultConnection or POSTGRES_* env vars).");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationCommandDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationCommandDbContext(optionsBuilder.Options);
    }

    static string BuildFromPostgresEnv()
    {
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5433";
        var db = Environment.GetEnvironmentVariable("POSTGRES_DB");
        var user = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var pass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        if (string.IsNullOrWhiteSpace(db) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            return string.Empty;
        }

        return $"Host={host};Port={port};Database={db};Username={user};Password={pass}";
    }
}
