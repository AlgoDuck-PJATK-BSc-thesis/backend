using AlgoDuck.DAL;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace AlgoDuck.Shared.Utilities.DependencyInitializers;

internal static class DbDependencyInitializer
{
    internal static void Initialize(WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing.");

        if (builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddDbContext<ApplicationCommandDbContext>(options => options.UseSqlite(connectionString));
            builder.Services.AddDbContext<ApplicationQueryDbContext>(options => options.UseSqlite(connectionString));
        }
        else
        {
            builder.Services.AddDbContext<ApplicationCommandDbContext>(options =>
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                }));

            builder.Services.AddDbContext<ApplicationQueryDbContext>(options =>
                options.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                }));
        }

        builder.Services.AddScoped<DataSeedingService>();

        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
            return ConnectionMultiplexer.Connect(configuration);
        });
        builder.Services.AddSingleton<IDatabase>(sp =>
        {
            var redis = sp.GetRequiredService<IConnectionMultiplexer>();
            return redis.GetDatabase();
        });
        
    }
}