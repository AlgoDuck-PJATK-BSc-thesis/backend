using WebApplication1.Modules.Contest.Services;
using WebApplication1.Modules.Contest.Repositories;
namespace WebApplication1.Modules.Contest;

public static class ContestModule
{
    public static IServiceCollection AddContestModule(this IServiceCollection services)
    {
        services.AddScoped<IContestRepository, ContestRepository>();
        services.AddScoped<IContestService, ContestService>();

        return services;
    }
}