using AlgoDuck.Modules.Cohort.Queries.GetCohortById;
using AlgoDuck.Modules.Cohort.Queries.GetUserCohorts;
using AlgoDuck.Modules.Cohort.Queries.GetCohortMembers;
using AlgoDuck.Modules.Cohort.Queries.GetCohortMessages;
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

        return services;
    }
}