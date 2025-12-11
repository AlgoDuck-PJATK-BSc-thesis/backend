using AlgoDuck.Modules.Cohort.Commands;
using AlgoDuck.Modules.Cohort.Queries;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Repositories;
using AlgoDuck.Modules.Cohort.Shared.Services;

namespace AlgoDuck.Modules.Cohort.Shared.Utils;

public static class CohortDependencyInitializer
{
    public static IServiceCollection AddCohortModule(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.Configure<ChatModerationSettings>(configuration.GetSection("ChatModeration"));
        services.Configure<ChatMediaSettings>(configuration.GetSection("ChatMedia"));
        services.Configure<ChatPresenceSettings>(configuration.GetSection("ChatPresence"));

        services.AddScoped<ICohortRepository, CohortRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

        services.AddHttpClient<IChatModerationService, ChatModerationService>();
        services.AddScoped<IChatMediaStorageService, ChatMediaStorageService>();
        services.AddSingleton<IChatPresenceService, ChatPresenceService>();

        services.AddCohortCommands();
        services.AddCohortQueries();

        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = environment.IsDevelopment();
        });

        return services;
    }

    public static void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddCohortModule(builder.Configuration, builder.Environment);
    }
}