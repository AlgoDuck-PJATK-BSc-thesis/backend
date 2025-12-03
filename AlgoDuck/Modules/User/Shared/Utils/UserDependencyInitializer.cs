using AlgoDuck.Modules.User.Queries.GetUserProfile;
using AlgoDuck.Modules.User.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Repositories;
using AlgoDuck.Modules.User.Shared.Services;

namespace AlgoDuck.Modules.User.Shared.Utils;

public static class UserDependencyInitializer
{
    public static IServiceCollection AddUserModule(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<IS3AvatarUrlGenerator, S3AvatarUrlGenerator>();
        services.AddScoped<IGetUserProfileHandler, GetUserProfileHandler>();
        services.AddScoped<GetUserProfileValidator>();

        return services;
    }
}