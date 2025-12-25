using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands;
using AlgoDuck.Modules.Auth.Queries;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using AlgoDuck.Modules.Auth.Shared.Middleware;
using AlgoDuck.Modules.Auth.Shared.Repositories;
using AlgoDuck.Modules.Auth.Shared.Services;
using AlgoDuck.Modules.Auth.Shared.Validators;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
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
    
    private static string Req(string? v, string key)
    {
        if (string.IsNullOrWhiteSpace(v)) throw new InvalidOperationException($"Missing configuration: {key}");
        return v;
    }

    internal static void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthModule(builder.Environment);

        
        var jwtConfig = builder.Configuration.GetSection("Jwt");
        var jwtKey = jwtConfig["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
        var jwtIssuer = jwtConfig["Issuer"] ?? "algoduck";
        var jwtAudience = jwtConfig["Audience"] ?? "algoduck-client";

        var validateIssuer = jwtConfig.GetValue("ValidateIssuer", builder.Environment.IsProduction());
        var validateAudience = jwtConfig.GetValue("ValidateAudience", builder.Environment.IsProduction());
        var validateLifetime = jwtConfig.GetValue("ValidateLifetime", true);
        var clockSkewSeconds = jwtConfig.GetValue("ClockSkewSeconds", 60);

        var jwtCookieName = jwtConfig.GetValue<string>("JwtCookieName") ?? "jwt";

        builder.Services
            .AddOptions<JwtSettings>()
            .Bind(builder.Configuration.GetSection("Jwt"))
            .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer))
            .Validate(s => !string.IsNullOrWhiteSpace(s.Audience))
            .ValidateOnStart();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;

                if (builder.Environment.IsProduction())
                {
                    options.Password.RequiredLength = 12;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                }
                else
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                }
            })
            .AddEntityFrameworkStores<ApplicationCommandDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = validateIssuer,
                    ValidateAudience = validateAudience,
                    ValidateLifetime = validateLifetime,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    ClockSkew = TimeSpan.FromSeconds(clockSkewSeconds),
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue(jwtCookieName, out var token) &&
                            !string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers["X-Token-Expired"] = "true";
                        }
                        return Task.CompletedTask;
                    }
                };
            })
            .AddGoogle("Google", o =>
            {
                var g = builder.Configuration.GetSection("Authentication:Google");
                o.ClientId =
                    Req(g["ClientId"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__GOOGLE__CLIENTID"),
                        "Authentication:Google:ClientId or AUTHENTICATION__GOOGLE__CLIENTID");
                o.ClientSecret =
                    Req(g["ClientSecret"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__GOOGLE__CLIENTSECRET"),
                        "Authentication:Google:ClientSecret or AUTHENTICATION__GOOGLE__CLIENTSECRET");
                o.CallbackPath = g["CallbackPath"] ?? "/api/auth/oauth/google";
                o.SaveTokens = true;
                o.CorrelationCookie.SameSite = SameSiteMode.Lax;
                if (builder.Environment.IsProduction()) o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                o.SignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddOAuth("GitHub", o =>
            {
                var gh = builder.Configuration.GetSection("Authentication:GitHub");
                o.ClientId =
                    Req(gh["ClientId"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__GITHUB__CLIENTID"),
                        "Authentication:GitHub:ClientId or AUTHENTICATION__GITHUB__CLIENTID");
                o.ClientSecret =
                    Req(
                        gh["ClientSecret"] ??
                        Environment.GetEnvironmentVariable("AUTHENTICATION__GITHUB__CLIENTSECRET"),
                        "Authentication:GitHub:ClientSecret or AUTHENTICATION__GITHUB__CLIENTSECRET");
                o.CallbackPath = gh["CallbackPath"] ?? "/api/auth/oauth/github";
                o.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                o.TokenEndpoint = "https://github.com/login/oauth/access_token";
                o.UserInformationEndpoint = "https://api.github.com/user";
                o.Scope.Add("read:user");
                o.Scope.Add("user:email");
                o.SaveTokens = true;
                o.CorrelationCookie.SameSite = SameSiteMode.Lax;
                if (builder.Environment.IsProduction()) o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                o.SignInScheme = IdentityConstants.ExternalScheme;
                o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                o.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                o.ClaimActions.MapJsonKey("urn:github:login", "login");
                o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

                o.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Headers.UserAgent.ParseAdd("AlgoDuckOAuth");
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                        var response = await context.Backchannel.SendAsync(
                            request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();

                        using var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                        context.RunClaimActions(user.RootElement);

                        var email = context.Identity?.FindFirst(ClaimTypes.Email)?.Value;
                        if (string.IsNullOrWhiteSpace(email))
                        {
                            var emailsReq =
                                new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
                            emailsReq.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            emailsReq.Headers.UserAgent.ParseAdd("AlgoDuckOAuth");
                            emailsReq.Headers.Authorization =
                                new AuthenticationHeaderValue("Bearer", context.AccessToken);

                            var emailsRes = await context.Backchannel.SendAsync(
                                emailsReq, HttpCompletionOption.ResponseHeadersRead,
                                context.HttpContext.RequestAborted);
                            if (emailsRes.IsSuccessStatusCode)
                            {
                                var json = await emailsRes.Content.ReadAsStringAsync();
                                var arr = JsonDocument.Parse(json).RootElement;
                                var best = arr.EnumerateArray()
                                    .Where(e => e.GetProperty("verified").GetBoolean())
                                    .OrderByDescending(e => e.GetProperty("primary").GetBoolean())
                                    .Select(e => e.GetProperty("email").GetString())
                                    .FirstOrDefault();
                                if (!string.IsNullOrWhiteSpace(best))
                                {
                                    context.Identity?.AddClaim(new Claim(ClaimTypes.Email, best));
                                }
                            }
                        }
                    }
                };
            })
            .AddFacebook("Facebook", o =>
            {
                var fb = builder.Configuration.GetSection("Authentication:Facebook");
                var appId = Req(
                    fb["AppId"] ?? fb["ClientId"] ??
                    Environment.GetEnvironmentVariable("AUTHENTICATION__FACEBOOK__APPID"),
                    "Authentication:Facebook:AppId/ClientId or AUTHENTICATION__FACEBOOK__APPID");
                var appSecret =
                    Req(
                        fb["AppSecret"] ?? fb["ClientSecret"] ??
                        Environment.GetEnvironmentVariable("AUTHENTICATION__FACEBOOK__APPSECRET"),
                        "Authentication:Facebook:AppSecret/ClientSecret or AUTHENTICATION__FACEBOOK__APPSECRET");
                o.AppId = appId;
                o.AppSecret = appSecret;
                o.CallbackPath = fb["CallbackPath"] ?? "/api/auth/oauth/facebook";
                o.SaveTokens = true;
                o.Scope.Add("email");
                o.Fields.Add("email");
                o.Fields.Add("name");
                o.CorrelationCookie.SameSite = SameSiteMode.Lax;
                if (builder.Environment.IsProduction()) o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                o.SignInScheme = IdentityConstants.ExternalScheme;
            });
        
        builder.Services.AddAuthorization();
        
        builder.Services.AddScoped<SharedTokenServiceInterface, SharedTokenService>();
    }

    public static IApplicationBuilder UseAuthModule(this IApplicationBuilder app)
    {
        app.UseMiddleware<AuthExceptionMiddleware>();
        app.UseMiddleware<TokenValidationMiddleware>();
        app.UseMiddleware<ApiKeyValidationMiddleware>();

        return app;
    }
}