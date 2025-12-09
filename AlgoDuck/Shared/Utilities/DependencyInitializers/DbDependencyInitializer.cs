using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Shared.Utilities.DependencyInitializers;

internal static class DbDependencyInitializer
{
    internal static void Initialize(WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing.");

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

        builder.Services.AddScoped<DataSeedingService>();

        builder.Services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationCommandDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddScoped<RoleManager<IdentityRole<Guid>>>();
        builder.Services.AddScoped<UserManager<ApplicationUser>>();

        builder.Services.AddScoped<JwtTokenProvider>();
    }
}