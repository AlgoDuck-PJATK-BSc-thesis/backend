using AlgoDuck.Modules.User.Queries.GetUserActivity;
using AlgoDuck.Modules.User.Queries.GetUserAchievements;
using AlgoDuck.Modules.User.Queries.GetUserById;
using AlgoDuck.Modules.User.Queries.GetUserProfile;
using AlgoDuck.Modules.User.Queries.GetUserSolvedProblems;
using AlgoDuck.Modules.User.Queries.GetUserStatistics;
using AlgoDuck.Modules.User.Queries.GetUserConfig;
using AlgoDuck.Modules.User.Queries.SearchUsers;
using AlgoDuck.Modules.User.Queries.GetUserRankings;
using AlgoDuck.Modules.User.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Repositories;
using AlgoDuck.Modules.User.Shared.Services;
using FluentValidation;

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

        services.AddScoped<IGetUserAchievementsHandler, GetUserAchievementsHandler>();
        services.AddScoped<GetUserAchievementsValidator>();

        services.AddScoped<IGetUserStatisticsHandler, GetUserStatisticsHandler>();
        services.AddScoped<GetUserStatisticsValidator>();

        services.AddScoped<IGetUserSolvedProblemsHandler, GetUserSolvedProblemsHandler>();
        services.AddScoped<GetUserSolvedProblemsValidator>();

        services.AddScoped<IGetUserActivityHandler, GetUserActivityHandler>();
        services.AddScoped<GetUserActivityValidator>();

        services.AddScoped<IGetUserByIdHandler, GetUserByIdHandler>();
        services.AddScoped<GetUserByIdValidator>();

        services.AddScoped<IGetUserConfigHandler, GetUserConfigHandler>();
        services.AddScoped<GetUserConfigValidator>();

        services.AddScoped<ISearchUsersHandler, SearchUsersHandler>();
        services.AddScoped<IValidator<SearchUsersDto>, SearchUsersValidator>();

        services.AddScoped<IGetUserRankingsHandler, GetUserRankingsHandler>();
        services.AddScoped<GetUserRankingsValidator>();

        return services;
    }
}