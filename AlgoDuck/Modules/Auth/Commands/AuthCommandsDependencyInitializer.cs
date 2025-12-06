using AlgoDuck.Modules.Auth.Commands.DisableTwoFactor;
using AlgoDuck.Modules.Auth.Commands.EnableTwoFactor;
using AlgoDuck.Modules.Auth.Commands.ExternalLogin;
using AlgoDuck.Modules.Auth.Commands.GenerateApiKey;
using AlgoDuck.Modules.Auth.Commands.Login;
using AlgoDuck.Modules.Auth.Commands.Logout;
using AlgoDuck.Modules.Auth.Commands.RefreshToken;
using AlgoDuck.Modules.Auth.Commands.Register;
using AlgoDuck.Modules.Auth.Commands.RequestPasswordReset;
using AlgoDuck.Modules.Auth.Commands.ResetPassword;
using AlgoDuck.Modules.Auth.Commands.RevokeApiKey;
using AlgoDuck.Modules.Auth.Commands.StartEmailVerification;
using AlgoDuck.Modules.Auth.Commands.VerifyEmail;
using AlgoDuck.Modules.Auth.Commands.VerifyTwoFactorLogin;
using AlgoDuck.Modules.Auth.Commands.ChangeEmailRequest;
using AlgoDuck.Modules.Auth.Commands.ChangeEmailConfirm;
using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands;

public static class AuthCommandsDependencyInitializer
{
    public static IServiceCollection AddAuthCommands(this IServiceCollection services)
    {
        services.AddScoped<IValidator<RegisterDto>, RegisterValidator>();
        services.AddScoped<IRegisterHandler, RegisterHandler>();

        services.AddScoped<IValidator<LoginDto>, LoginValidator>();
        services.AddScoped<ILoginHandler, LoginHandler>();

        services.AddScoped<IValidator<RefreshTokenDto>, RefreshTokenValidator>();
        services.AddScoped<IRefreshTokenHandler, RefreshTokenHandler>();

        services.AddScoped<IValidator<LogoutDto>, LogoutValidator>();
        services.AddScoped<ILogoutHandler, LogoutHandler>();

        services.AddScoped<IValidator<StartEmailVerificationDto>, StartEmailVerificationValidator>();
        services.AddScoped<IStartEmailVerificationHandler, StartEmailVerificationHandler>();

        services.AddScoped<IValidator<VerifyEmailDto>, VerifyEmailValidator>();
        services.AddScoped<IVerifyEmailHandler, VerifyEmailHandler>();

        services.AddScoped<IValidator<RequestPasswordResetDto>, RequestPasswordResetValidator>();
        services.AddScoped<IRequestPasswordResetHandler, RequestPasswordResetHandler>();

        services.AddScoped<IValidator<ResetPasswordDto>, ResetPasswordValidator>();
        services.AddScoped<IResetPasswordHandler, ResetPasswordHandler>();

        services.AddScoped<IValidator<VerifyTwoFactorLoginDto>, VerifyTwoFactorLoginValidator>();
        services.AddScoped<IVerifyTwoFactorLoginHandler, VerifyTwoFactorLoginHandler>();

        services.AddScoped<IValidator<EnableTwoFactorDto>, EnableTwoFactorValidator>();
        services.AddScoped<IEnableTwoFactorHandler, EnableTwoFactorHandler>();

        services.AddScoped<IValidator<DisableTwoFactorDto>, DisableTwoFactorValidator>();
        services.AddScoped<IDisableTwoFactorHandler, DisableTwoFactorHandler>();

        services.AddScoped<IValidator<GenerateApiKeyDto>, GenerateApiKeyValidator>();
        services.AddScoped<IGenerateApiKeyHandler, GenerateApiKeyHandler>();

        services.AddScoped<IValidator<RevokeApiKeyDto>, RevokeApiKeyValidator>();
        services.AddScoped<IRevokeApiKeyHandler, RevokeApiKeyHandler>();

        services.AddScoped<IValidator<ExternalLoginDto>, ExternalLoginValidator>();
        services.AddScoped<IExternalLoginHandler, ExternalLoginHandler>();
        
        services.AddScoped<IValidator<ChangeEmailRequestDto>, ChangeEmailRequestValidator>();
        services.AddScoped<IChangeEmailRequestHandler, ChangeEmailRequestHandler>();

        services.AddScoped<IValidator<ChangeEmailConfirmDto>, ChangeEmailConfirmValidator>();
        services.AddScoped<IChangeEmailConfirmHandler, ChangeEmailConfirmHandler>();

        return services;
    }
}