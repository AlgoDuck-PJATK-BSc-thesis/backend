using AlgoDuck.Modules.Auth.Commands.ApiKeys.GenerateApiKey;
using AlgoDuck.Modules.Auth.Commands.ApiKeys.RevokeApiKey;
using AlgoDuck.Modules.Auth.Commands.Email.ChangeEmailConfirm;
using AlgoDuck.Modules.Auth.Commands.Email.ChangeEmailRequest;
using AlgoDuck.Modules.Auth.Commands.Email.StartEmailVerification;
using AlgoDuck.Modules.Auth.Commands.Email.VerifyEmail;
using AlgoDuck.Modules.Auth.Commands.Login.ExternalLogin;
using AlgoDuck.Modules.Auth.Commands.Login.Login;
using AlgoDuck.Modules.Auth.Commands.Login.Logout;
using AlgoDuck.Modules.Auth.Commands.Login.Register;
using AlgoDuck.Modules.Auth.Commands.Password.RequestPasswordReset;
using AlgoDuck.Modules.Auth.Commands.Password.ResetPassword;
using AlgoDuck.Modules.Auth.Commands.Session.RefreshToken;
using AlgoDuck.Modules.Auth.Commands.Session.RevokeOtherSessions;
using AlgoDuck.Modules.Auth.Commands.Session.RevokeSession;
using AlgoDuck.Modules.Auth.Commands.TwoFactor.DisableTwoFactor;
using AlgoDuck.Modules.Auth.Commands.TwoFactor.EnableTwoFactor;
using AlgoDuck.Modules.Auth.Commands.TwoFactor.VerifyTwoFactorLogin;
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

        services.AddScoped<IEnableTwoFactorHandler, EnableTwoFactorHandler>();

        services.AddScoped<IDisableTwoFactorHandler, DisableTwoFactorHandler>();

        services.AddScoped<IValidator<GenerateApiKeyDto>, GenerateApiKeyValidator>();
        services.AddScoped<IGenerateApiKeyHandler, GenerateApiKeyHandler>();

        services.AddScoped<IRevokeApiKeyHandler, RevokeApiKeyHandler>();

        services.AddScoped<IValidator<ExternalLoginDto>, ExternalLoginValidator>();
        services.AddScoped<IExternalLoginHandler, ExternalLoginHandler>();

        services.AddScoped<IValidator<ChangeEmailRequestDto>, ChangeEmailRequestValidator>();
        services.AddScoped<IChangeEmailRequestHandler, ChangeEmailRequestHandler>();

        services.AddScoped<IValidator<ChangeEmailConfirmDto>, ChangeEmailConfirmValidator>();
        services.AddScoped<IChangeEmailConfirmHandler, ChangeEmailConfirmHandler>();

        services.AddScoped<IValidator<RevokeSessionDto>, RevokeSessionValidator>();
        services.AddScoped<IRevokeSessionHandler, RevokeSessionHandler>();

        services.AddScoped<IValidator<RevokeOtherSessionsDto>, RevokeOtherSessionsValidator>();
        services.AddScoped<IRevokeOtherSessionsHandler, RevokeOtherSessionsHandler>();

        return services;
    }
}
