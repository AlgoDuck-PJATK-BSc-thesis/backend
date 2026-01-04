using AlgoDuck.Modules.User.Queries.Admin.GetUsers;
using AlgoDuck.Modules.User.Queries.User.Activity.GetUserAchievements;
using AlgoDuck.Modules.User.Queries.User.Activity.GetUserActivity;
using AlgoDuck.Modules.User.Queries.User.Leaderboard.GetCohortLeaderboard;
using AlgoDuck.Modules.User.Queries.User.Leaderboard.GetLeaderboardGlobal;
using AlgoDuck.Modules.User.Queries.User.Leaderboard.GetUserLeaderboardPosition;
using AlgoDuck.Modules.User.Queries.User.Leaderboard.GetUserRankings;
using AlgoDuck.Modules.User.Queries.User.Profile.GetSelectedAvatar;
using AlgoDuck.Modules.User.Queries.User.Profile.GetUserById;
using AlgoDuck.Modules.User.Queries.User.Profile.GetUserProfile;
using AlgoDuck.Modules.User.Queries.User.Profile.GetVerifiedEmail;
using AlgoDuck.Modules.User.Queries.User.Settings.GetTwoFactorEnabled;
using AlgoDuck.Modules.User.Queries.User.Settings.GetUserConfig;
using AlgoDuck.Modules.User.Queries.User.Stats.GetUserSolvedProblems;
using AlgoDuck.Modules.User.Queries.User.Stats.GetUserStatistics;
using ISearchUsersHandler = AlgoDuck.Modules.User.Queries.Admin.SearchUsers.ISearchUsersHandler;
using SearchUsersDto = AlgoDuck.Modules.User.Queries.Admin.SearchUsers.SearchUsersDto;
using SearchUsersHandler = AlgoDuck.Modules.User.Queries.Admin.SearchUsers.SearchUsersHandler;
using SearchUsersValidator = AlgoDuck.Modules.User.Queries.Admin.SearchUsers.SearchUsersValidator;
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

        services.AddScoped<User.Profile.SearchUsers.ISearchUsersHandler, User.Profile.SearchUsers.SearchUsersHandler>();
        services.AddScoped<IValidator<User.Profile.SearchUsers.SearchUsersDto>, User.Profile.SearchUsers.SearchUsersValidator>();

        services.AddScoped<IGetUserRankingsHandler, GetUserRankingsHandler>();
        services.AddScoped<GetUserRankingsValidator>();

        services.AddScoped<IGetUserLeaderboardPositionHandler, GetUserLeaderboardPositionHandler>();

        services.AddScoped<IGetVerifiedEmailHandler, GetVerifiedEmailHandler>();

        services.AddScoped<IGetTwoFactorEnabledHandler, GetTwoFactorEnabledHandler>();

        services.AddScoped<IGetSelectedAvatarHandler, GetSelectedAvatarHandler>();

        services.AddScoped<IGetLeaderboardGlobalHandler, GetLeaderboardGlobalHandler>();

        services.AddScoped<IGetCohortLeaderboardHandler, GetCohortLeaderboardHandler>();

        services.AddScoped<IGetUsersHandler, GetUsersHandler>();
        services.AddScoped<IValidator<GetUsersDto>, GetUsersValidator>();

        services.AddScoped<ISearchUsersHandler, SearchUsersHandler>();
        services.AddScoped<IValidator<SearchUsersDto>, SearchUsersValidator>();

        return services;
    }
}
