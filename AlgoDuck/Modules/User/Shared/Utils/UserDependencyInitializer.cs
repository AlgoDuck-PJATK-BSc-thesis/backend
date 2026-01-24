using AlgoDuck.Modules.User.Commands;
using AlgoDuck.Modules.User.Queries;
using AlgoDuck.Modules.User.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Repositories;
using AlgoDuck.Modules.User.Shared.Services;
using AlgoDuck.Modules.User.Shared.Reminders;

namespace AlgoDuck.Modules.User.Shared.Utils;

public static class UserDependencyInitializer
{
    public static IServiceCollection AddUserModule(this IServiceCollection services)
    {
        services.AddScoped<IDefaultDuckService, DefaultDuckService>();
        services.AddScoped<IUserAchievementSyncService, UserAchievementSyncService>();
        services.AddScoped<IUserBootstrapperService, UserBootstrapperService>();

        services.AddScoped<IAchievementService, AchievementService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IS3AvatarUrlGenerator, S3AvatarUrlGenerator>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddSingleton<ReminderNextAtCalculator>();
        services.AddScoped<IReminderEmailSender, ReminderEmailSender>();
        services.AddHostedService<ReminderHostedService>();

        services.AddUserCommands();
        services.AddUserQueries();

        return services;
    }

    public static void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddUserModule();
    }
}