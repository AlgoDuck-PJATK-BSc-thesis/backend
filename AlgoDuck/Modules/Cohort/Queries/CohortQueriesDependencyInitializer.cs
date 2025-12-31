using AlgoDuck.Modules.Cohort.Queries.GetCohortActiveMembers;
using AlgoDuck.Modules.Cohort.Queries.GetCohortById;
using AlgoDuck.Modules.Cohort.Queries.GetCohortMembers;
using AlgoDuck.Modules.Cohort.Queries.GetCohortMessages;
using AlgoDuck.Modules.Cohort.Queries.GetUserCohorts;
using AlgoDuck.Modules.Cohort.Queries.AdminGetCohorts;
using AlgoDuck.Modules.Cohort.Queries.AdminSearchCohorts;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Queries;

public static class CohortQueriesDependencyInitializer
{
    public static IServiceCollection AddCohortQueries(this IServiceCollection services)
    {
        services.AddScoped<IValidator<GetCohortByIdRequestDto>, GetCohortByIdValidator>();
        services.AddScoped<IGetCohortByIdHandler, GetCohortByIdHandler>();

        services.AddScoped<IGetUserCohortsHandler, GetUserCohortsHandler>();

        services.AddScoped<IValidator<GetCohortMembersRequestDto>, GetCohortMembersValidator>();
        services.AddScoped<IGetCohortMembersHandler, GetCohortMembersHandler>();

        services.AddScoped<IValidator<GetCohortMessagesRequestDto>, GetCohortMessagesValidator>();
        services.AddScoped<IGetCohortMessagesHandler, GetCohortMessagesHandler>();

        services.AddScoped<IValidator<GetCohortActiveMembersRequestDto>, GetCohortActiveMembersValidator>();
        services.AddScoped<IGetCohortActiveMembersHandler, GetCohortActiveMembersHandler>();

        services.AddScoped<IAdminGetCohortsHandler, AdminGetCohortsHandler>();
        services.AddScoped<IValidator<AdminGetCohortsDto>, AdminGetCohortsValidator>();

        services.AddScoped<IAdminSearchCohortsHandler, AdminSearchCohortsHandler>();
        services.AddScoped<IValidator<AdminSearchCohortsDto>, AdminSearchCohortsValidator>();

        return services;
    }
}