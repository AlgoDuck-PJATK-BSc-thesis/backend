using AlgoDuck.Modules.User.Commands;
using AlgoDuck.Modules.User.Queries;
using AlgoDuck.Modules.User.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Repositories;
using AlgoDuck.Modules.User.Shared.Services;

namespace AlgoDuck.Modules.User.Shared.Utils;

public static class UserDependencyInitializer
{
    public static IServiceCollection AddUserModule(this IServiceCollection services)
    {
        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IS3AvatarUrlGenerator, S3AvatarUrlGenerator>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddUserCommands();
        services.AddUserQueries();

        return services;
    }

    public static void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddUserModule();
    }
}