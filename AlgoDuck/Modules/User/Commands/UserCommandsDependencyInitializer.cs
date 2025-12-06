using AlgoDuck.Modules.User.Commands.ChangePassword;
using AlgoDuck.Modules.User.Commands.DeleteAccount;
using AlgoDuck.Modules.User.Commands.SelectAvatar;
using AlgoDuck.Modules.User.Commands.SetEditorTheme;
using AlgoDuck.Modules.User.Commands.UpdatePreferences;
using AlgoDuck.Modules.User.Commands.UpdateUsername;
using AlgoDuck.Modules.User.Commands.SetEditorLayout;
using FluentValidation;

namespace AlgoDuck.Modules.User.Commands;

public static class UserCommandsDependencyInitializer
{
    public static IServiceCollection AddUserCommands(this IServiceCollection services)
    {
        services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordValidator>();
        services.AddScoped<IChangePasswordHandler, ChangePasswordHandler>();

        services.AddScoped<IValidator<UpdatePreferencesDto>, UpdatePreferencesValidator>();
        services.AddScoped<IUpdatePreferencesHandler, UpdatePreferencesHandler>();

        services.AddScoped<IValidator<UpdateUsernameDto>, UpdateUsernameValidator>();
        services.AddScoped<IUpdateUsernameHandler, UpdateUsernameHandler>();

        services.AddScoped<IValidator<DeleteAccountDto>, DeleteAccountValidator>();
        services.AddScoped<IDeleteAccountHandler, DeleteAccountHandler>();

        services.AddScoped<IValidator<SelectAvatarDto>, SelectAvatarValidator>();
        services.AddScoped<ISelectAvatarHandler, SelectAvatarHandler>();

        services.AddScoped<IValidator<SetEditorThemeDto>, SetEditorThemeValidator>();
        services.AddScoped<ISetEditorThemeHandler, SetEditorThemeHandler>();
        
        services.AddScoped<IValidator<SetEditorLayoutDto>, SetEditorLayoutValidator>();
        services.AddScoped<ISetEditorLayoutHandler, SetEditorLayoutHandler>();

        return services;
    }
}