using AlgoDuck.Modules.Cohort.Commands.CohortManagement.CreateCohort;
using AlgoDuck.Modules.Cohort.Commands.CohortManagement.UpdateCohort;
using AlgoDuck.Modules.Cohort.Commands.CohortManagement.JoinCohort;
using AlgoDuck.Modules.Cohort.Commands.CohortManagement.LeaveCohort;
using AlgoDuck.Modules.Cohort.Commands.Chat.SendMessage;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands;

public static class CohortCommandsDependencyInitializer
{
    public static IServiceCollection AddCohortCommands(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateCohortDto>, CreateCohortValidator>();
        services.AddScoped<ICreateCohortHandler, CreateCohortHandler>();

        services.AddScoped<IValidator<UpdateCohortDto>, UpdateCohortValidator>();
        services.AddScoped<IUpdateCohortHandler, UpdateCohortHandler>();

        services.AddScoped<IJoinCohortHandler, JoinCohortHandler>();
        services.AddScoped<ILeaveCohortHandler, LeaveCohortHandler>();

        services.AddScoped<IValidator<SendMessageDto>, SendMessageValidator>();
        services.AddScoped<ISendMessageHandler, SendMessageHandler>();

        return services;
    }
}