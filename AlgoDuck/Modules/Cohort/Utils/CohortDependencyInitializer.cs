using AlgoDuck.Modules.Cohort.CohortManagement.Shared;
using AlgoDuck.Modules.Cohort.Interfaces;
using AlgoDuck.Modules.Cohort.Services;

namespace AlgoDuck.Modules.Cohort.Utils;

internal static class CohortDependencyInitializer
{
    internal static void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICohortService, CohortService>();
        builder.Services.AddScoped<ICohortChatService, CohortChatService>();
        builder.Services.AddScoped<ICohortLeaderboardService, CohortLeaderboardService>();

        builder.Services.AddScoped<ICohortRepository, CohortRepository>();    
        
        
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = builder.Environment.IsDevelopment();
        });
    }
}