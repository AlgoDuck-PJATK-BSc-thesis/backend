using AlgoDuck.Modules.Contest.Repositories;
using AlgoDuck.Modules.Contest.Services;

namespace AlgoDuck.Modules.Contest;

public static class ContestModule
{
    public static IServiceCollection AddContestModule(this IServiceCollection services)
    {
        services.AddScoped<IContestRepository, ContestRepository>();
        services.AddScoped<IContestService, ContestService>();

        return services;
    }
}