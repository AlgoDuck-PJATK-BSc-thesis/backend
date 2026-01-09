using AlgoDuck.DAL;
using AlgoDuck.Shared.S3;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace AlgoDuck.Shared.Utilities.DependencyInitializers;

internal static class DbDependencyInitializer
{
    internal static void Initialize(WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing.");

        builder.Services.AddDbContext<ApplicationCommandDbContext>(options =>
            options.UseNpgsql(connectionString));

        builder.Services.AddDbContext<ApplicationQueryDbContext>(options =>
            options.UseNpgsql(connectionString));

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
        
        
        builder.Services.Configure<RedisCachePrefixes>(
            builder.Configuration.GetSection("RedisCachePrefixes"));
        
        
        
    }
}