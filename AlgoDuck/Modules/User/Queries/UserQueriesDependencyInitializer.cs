using AlgoDuck.Modules.User.Queries.GetCohortLeaderboard;
using AlgoDuck.Modules.User.Queries.GetLeaderboardGlobal;
using AlgoDuck.Modules.User.Queries.GetSelectedAvatar;
using AlgoDuck.Modules.User.Queries.GetTwoFactorEnabled;
using AlgoDuck.Modules.User.Queries.GetUserActivity;
using AlgoDuck.Modules.User.Queries.GetUserAchievements;
using AlgoDuck.Modules.User.Queries.GetUserById;
using AlgoDuck.Modules.User.Queries.GetUserProfile;
using AlgoDuck.Modules.User.Queries.GetUserSolvedProblems;
using AlgoDuck.Modules.User.Queries.GetUserStatistics;
using AlgoDuck.Modules.User.Queries.GetUserConfig;
using AlgoDuck.Modules.User.Queries.GetUserLeaderboardPosition;
using AlgoDuck.Modules.User.Queries.SearchUsers;
using AlgoDuck.Modules.User.Queries.GetUserRankings;
using AlgoDuck.Modules.User.Queries.GetVerifiedEmail;
using FluentValidation;

namespace AlgoDuck.Modules.User.Queries;

public static class UserQueriesDependencyInitializer
{
    public static IServiceCollection AddUserQueries(this IServiceCollection services)
    {
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
        
        services.AddScoped<IGetUserLeaderboardPositionHandler, GetUserLeaderboardPositionHandler>();
        
        services.AddScoped<IGetVerifiedEmailHandler, GetVerifiedEmailHandler>();
        
        services.AddScoped<IGetTwoFactorEnabledHandler, GetTwoFactorEnabledHandler>();
        
        services.AddScoped<IGetSelectedAvatarHandler, GetSelectedAvatarHandler>();
        
        services.AddScoped<IGetLeaderboardGlobalHandler, GetLeaderboardGlobalHandler>();
        
        services.AddScoped<IGetCohortLeaderboardHandler, GetCohortLeaderboardHandler>();
        
        return services;
    }
}