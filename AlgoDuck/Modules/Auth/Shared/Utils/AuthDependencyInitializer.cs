using AlgoDuck.Modules.Auth.Interfaces;
using AlgoDuck.Modules.Auth.Services;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Middleware;
using AlgoDuck.Modules.Auth.Shared.Repositories;
using AlgoDuck.Modules.Auth.Shared.Services;
using AlgoDuck.Modules.Auth.Shared.Validators;
using Microsoft.AspNetCore.Identity;
using SharedEmailSender = AlgoDuck.Modules.Auth.Shared.Interfaces.IEmailSender;
using SharedEmailTransport = AlgoDuck.Modules.Auth.Shared.Interfaces.IEmailTransport;
using SharedPostmarkEmailSender = AlgoDuck.Modules.Auth.Shared.Email.PostmarkEmailSender;
using SharedTokenServiceInterface = AlgoDuck.Modules.Auth.Shared.Interfaces.ITokenService;
using SharedTokenService = AlgoDuck.Modules.Auth.Shared.Services.TokenService;
using SharedTwoFactorService = AlgoDuck.Modules.Auth.Shared.Services.TwoFactorService;
using CoreTwoFactorService = AlgoDuck.Modules.Auth.TwoFactor.TwoFactorService;

namespace AlgoDuck.Modules.Auth.Shared.Utils;

public static class AuthDependencyInitializer
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddScoped<TokenUtility>();

        services.AddScoped<EmailValidator>();
        services.AddScoped<PasswordValidator>();
        services.AddScoped<ApiKeyValidator>();
        services.AddScoped<PermissionValidator>();
        services.AddScoped<TokenValidator>();

        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IPermissionsRepository, PermissionsRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ITokenRepository, TokenRepository>();

        services.AddScoped<SharedEmailTransport, SharedPostmarkEmailSender>();
        services.AddScoped<SharedEmailSender, EmailSender>();

        services.AddScoped(typeof(IPasswordHasher<>), typeof(PasswordHasher<>));

        services.AddScoped<IPermissionsService, PermissionsService>();
        services.AddScoped<SharedTokenServiceInterface, SharedTokenService>();
        services.AddScoped<AlgoDuck.Modules.Auth.TwoFactor.ITwoFactorService, CoreTwoFactorService>();
        services.AddScoped<SharedTwoFactorService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IAuthValidator, AuthValidator>();
        services.AddScoped<SessionService>();
        services.AddScoped<ExternalAuthService>();

        if (environment.IsDevelopment())
        {
            services.AddScoped<IAuthService, DevelopmentAuthService>();
        }
        else
        {
            services.AddScoped<IAuthService, ProductionAuthService>();
        }

        return services;
    }

    public static IApplicationBuilder UseAuthModule(this IApplicationBuilder app)
    {
        app.UseMiddleware<AuthExceptionMiddleware>();
        app.UseMiddleware<TokenValidationMiddleware>();
        app.UseMiddleware<ApiKeyValidationMiddleware>();

        return app;
    }
}