using AlgoDuck.Modules.Auth.Commands;
using AlgoDuck.Modules.Auth.Queries;
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
        services.AddScoped<ITwoFactorService, TwoFactorService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IAuthValidator, AuthValidator>();
        services.AddScoped<SessionService>();
        services.AddScoped<ExternalAuthService>();
        
        services.AddScoped<IExternalAuthProvider, DevExternalAuthProvider>();

        services.AddAuthCommands();
        services.AddAuthQueries();

        return services;
    }

    public static void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthModule(builder.Environment);
    }

    public static IApplicationBuilder UseAuthModule(this IApplicationBuilder app)
    {
        app.UseMiddleware<AuthExceptionMiddleware>();
        app.UseMiddleware<TokenValidationMiddleware>();
        app.UseMiddleware<ApiKeyValidationMiddleware>();

        return app;
    }
}