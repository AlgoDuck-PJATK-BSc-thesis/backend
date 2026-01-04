using AlgoDuck.Modules.User.Commands.CreateUser;
using AlgoDuck.Modules.User.Commands.DeleteUser;
using AlgoDuck.Modules.User.Commands.UpdateUser;
using AlgoDuck.Modules.User.Commands.User.Account.ChangePassword;
using AlgoDuck.Modules.User.Commands.User.Account.DeleteAccount;
using AlgoDuck.Modules.User.Commands.User.Preferences.SetEditorLayout;
using AlgoDuck.Modules.User.Commands.User.Preferences.SetEditorTheme;
using AlgoDuck.Modules.User.Commands.User.Preferences.UpdatePreferences;
using AlgoDuck.Modules.User.Commands.User.Profile.SelectAvatar;
using AlgoDuck.Modules.User.Commands.User.Profile.UpdateUsername;
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

        services.AddScoped<IDeleteUserHandler, DeleteUserHandler>();

        services.AddScoped<IValidator<CreateUserDto>, CreateUserValidator>();
        services.AddScoped<ICreateUserHandler, CreateUserHandler>();

        services.AddScoped<IValidator<UpdateUserDto>, UpdateUserValidator>();
        services.AddScoped<IUpdateUserHandler, UpdateUserHandler>();

        return services;
    }
}
