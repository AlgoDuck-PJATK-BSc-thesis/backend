using AlgoDuck.Modules.Cohort.Commands.AdminCohortMembers.AddCohortMember;
using AlgoDuck.Modules.Cohort.Commands.AdminCohortMembers.RemoveCohortMember;
using AlgoDuck.Modules.Cohort.Commands.Chat.SendMessage;
using AlgoDuck.Modules.Cohort.Commands.Chat.UploadMedia;
using AlgoDuck.Modules.Cohort.Commands.CohortManagement.CreateCohort;
using AlgoDuck.Modules.Cohort.Commands.CohortManagement.JoinCohort;
using AlgoDuck.Modules.Cohort.Commands.CohortManagement.LeaveCohort;
using AlgoDuck.Modules.Cohort.Commands.CohortManagement.UpdateCohort;
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

        services.AddScoped<IValidator<AdminAddCohortMemberDto>, AdminAddCohortMemberValidator>();
        services.AddScoped<IAdminAddCohortMemberHandler, AdminAddCohortMemberHandler>();

        services.AddScoped<IAdminRemoveCohortMemberHandler, AdminRemoveCohortMemberHandler>();

        services.AddScoped<IValidator<SendMessageDto>, SendMessageValidator>();
        services.AddScoped<ISendMessageHandler, SendMessageHandler>();

        services.AddScoped<IValidator<UploadMediaDto>, UploadMediaValidator>();
        services.AddScoped<IUploadMediaHandler, UploadMediaHandler>();

        return services;
    }
}