using AlgoDuck.Modules.Cohort.Commands.Admin.Members.AddCohortMember;
using AlgoDuck.Modules.Cohort.Commands.Admin.Members.RemoveCohortMember;
using AdminCreate = AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.CreateCohort;
using AdminDelete = AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.DeleteCohort;
using AdminUpdate = AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.UpdateCohort;
using UserCreate = AlgoDuck.Modules.Cohort.Commands.User.Management.CreateCohort;
using AlgoDuck.Modules.Cohort.Commands.User.Chat.SendMessage;
using AlgoDuck.Modules.Cohort.Commands.User.Chat.UploadMedia;
using AlgoDuck.Modules.Cohort.Commands.User.Management.JoinCohort;
using AlgoDuck.Modules.Cohort.Commands.User.Management.JoinCohortByCode;
using AlgoDuck.Modules.Cohort.Commands.User.Management.LeaveCohort;
using UserUpdate = AlgoDuck.Modules.Cohort.Commands.User.Management.UpdateCohort;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands;

public static class CohortCommandsDependencyInitializer
{
    public static IServiceCollection AddCohortCommands(this IServiceCollection services)
    {
        services.AddScoped<IValidator<UserCreate.CreateCohortDto>, UserCreate.CreateCohortValidator>();
        services.AddScoped<UserCreate.ICreateCohortHandler, UserCreate.CreateCohortHandler>();

        services.AddScoped<IValidator<UserUpdate.UpdateCohortDto>, UserUpdate.UpdateCohortValidator>();
        services.AddScoped<UserUpdate.IUpdateCohortHandler, UserUpdate.UpdateCohortHandler>();

        services.AddScoped<IValidator<AdminCreate.CreateCohortDto>, AdminCreate.CreateCohortValidator>();
        services.AddScoped<AdminCreate.ICreateCohortHandler, AdminCreate.CreateCohortHandler>();

        services.AddScoped<IValidator<AdminUpdate.UpdateCohortDto>, AdminUpdate.UpdateCohortValidator>();
        services.AddScoped<AdminUpdate.IUpdateCohortHandler, AdminUpdate.UpdateCohortHandler>();

        services.AddScoped<AdminDelete.IDeleteCohortHandler, AdminDelete.DeleteCohortHandler>();

        services.AddScoped<IJoinCohortHandler, JoinCohortHandler>();
        services.AddScoped<ILeaveCohortHandler, LeaveCohortHandler>();

        services.AddScoped<IValidator<AddCohortMemberDto>, AddCohortMemberValidator>();
        services.AddScoped<IAddCohortMemberHandler, AddCohortMemberHandler>();

        services.AddScoped<IRemoveCohortMemberHandler, RemoveCohortMemberHandler>();

        services.AddScoped<IValidator<SendMessageDto>, SendMessageValidator>();
        services.AddScoped<ISendMessageHandler, SendMessageHandler>();

        services.AddScoped<IValidator<UploadMediaDto>, UploadMediaValidator>();
        services.AddScoped<IUploadMediaHandler, UploadMediaHandler>();

        services.AddScoped<IJoinCohortByCodeHandler, JoinCohortByCodeHandler>();
        services.AddScoped<IValidator<JoinCohortByCodeDto>, JoinCohortByCodeValidator>();

        return services;
    }
}
