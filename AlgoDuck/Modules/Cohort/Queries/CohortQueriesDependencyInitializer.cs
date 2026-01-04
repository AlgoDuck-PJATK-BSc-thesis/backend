using AlgoDuck.Modules.Cohort.Queries.Admin.Members.GetCohortMembers;
using AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.GetCohorts;
using AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.SearchCohorts;
using AlgoDuck.Modules.Cohort.Queries.User.Chat.GetCohortMessages;
using AlgoDuck.Modules.Cohort.Queries.User.Cohorts.GetCohortById;
using AlgoDuck.Modules.Cohort.Queries.User.Cohorts.GetUserCohorts;
using AlgoDuck.Modules.Cohort.Queries.User.Members.GetCohortActiveMembers;
using AlgoDuck.Modules.Cohort.Queries.User.Members.GetCohortMembers;
using GetCohortMembersHandler = AlgoDuck.Modules.Cohort.Queries.Admin.Members.GetCohortMembers.GetCohortMembersHandler;
using GetCohortMembersRequestDto = AlgoDuck.Modules.Cohort.Queries.Admin.Members.GetCohortMembers.GetCohortMembersRequestDto;
using GetCohortMembersValidator = AlgoDuck.Modules.Cohort.Queries.Admin.Members.GetCohortMembers.GetCohortMembersValidator;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Queries;

public static class CohortQueriesDependencyInitializer
{
    public static IServiceCollection AddCohortQueries(this IServiceCollection services)
    {
        services.AddScoped<IValidator<GetCohortByIdRequestDto>, GetCohortByIdValidator>();
        services.AddScoped<IGetCohortByIdHandler, GetCohortByIdHandler>();

        services.AddScoped<IGetUserCohortsHandler, GetUserCohortsHandler>();

        services.AddScoped<IValidator<User.Members.GetCohortMembers.GetCohortMembersRequestDto>, User.Members.GetCohortMembers.GetCohortMembersValidator>();
        services.AddScoped<IGetCohortMembersHandler, User.Members.GetCohortMembers.GetCohortMembersHandler>();

        services.AddScoped<IValidator<GetCohortMessagesRequestDto>, GetCohortMessagesValidator>();
        services.AddScoped<IGetCohortMessagesHandler, GetCohortMessagesHandler>();

        services.AddScoped<IValidator<GetCohortActiveMembersRequestDto>, GetCohortActiveMembersValidator>();
        services.AddScoped<IGetCohortActiveMembersHandler, GetCohortActiveMembersHandler>();

        services.AddScoped<IAdminGetCohortsHandler, AdminGetCohortsHandler>();
        services.AddScoped<IValidator<AdminGetCohortsDto>, AdminGetCohortsValidator>();

        services.AddScoped<ISearchCohortsHandler, SearchCohortsHandler>();
        services.AddScoped<IValidator<SearchCohortsDto>, SearchCohortsValidator>();

        services.AddScoped<IAdminGetCohortMembersHandler, GetCohortMembersHandler>();
        services.AddScoped<IValidator<GetCohortMembersRequestDto>, GetCohortMembersValidator>();

        return services;
    }
}
