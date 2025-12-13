using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Commands;
using AlgoDuck.Modules.Auth.Queries;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Repositories;
using AlgoDuck.Modules.Auth.Shared.Services;
using AlgoDuck.Modules.Auth.Shared.Validators;
using AlgoDuck.Modules.Auth.Shared.Jwt;
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
    private static string Req(string? v, string key)
    {
        if (string.IsNullOrWhiteSpace(v)) throw new InvalidOperationException($"Missing configuration: {key}");
        return v;
    }

    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("Jwt"))
            .PostConfigure(s =>
            {
                if (string.IsNullOrWhiteSpace(s.SigningKey))
                {
                    var fallback = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY");
                    if (!string.IsNullOrWhiteSpace(fallback))
                    {
                        s.SigningKey = fallback;
                    }
                }

                if (string.IsNullOrWhiteSpace(s.SigningKey) && environment.IsProduction())
                {
                    throw new InvalidOperationException("Jwt:SigningKey is missing. Set env var Jwt__SigningKey (recommended) or JWT_SIGNING_KEY (fallback).");
                }

                if (string.IsNullOrWhiteSpace(s.Issuer)) s.Issuer = "algoduck";
                if (string.IsNullOrWhiteSpace(s.Audience)) s.Audience = "algoduck-client";
                if (s.AccessTokenMinutes <= 0) s.AccessTokenMinutes = 15;
                if (s.RefreshTokenMinutes <= 0) s.RefreshTokenMinutes = 60 * 24 * 7;
                if (string.IsNullOrWhiteSpace(s.AccessTokenCookieName)) s.AccessTokenCookieName = "jwt";
                if (string.IsNullOrWhiteSpace(s.RefreshTokenCookieName)) s.RefreshTokenCookieName = "refresh_token";
                if (string.IsNullOrWhiteSpace(s.CsrfCookieName)) s.CsrfCookieName = "csrf_token";
                if (string.IsNullOrWhiteSpace(s.CsrfHeaderName)) s.CsrfHeaderName = "X-CSRF-Token";
            })
            .Validate(s => !environment.IsProduction() || s.SigningKey.Length >= 32, "Jwt signing key must be at least 32 characters.")
            .ValidateOnStart();

        var jwtConfig = configuration.GetSection("Jwt");

        var jwtKey =
            jwtConfig["SigningKey"] ??
            jwtConfig["Key"] ??
            Environment.GetEnvironmentVariable("JWT_SIGNING_KEY") ??
            string.Empty;

        if (string.IsNullOrWhiteSpace(jwtKey) && environment.IsProduction())
        {
            throw new InvalidOperationException("Jwt signing key is missing. Set env var Jwt__SigningKey (recommended) or JWT_SIGNING_KEY (fallback).");
        }

        var jwtIssuer = jwtConfig["Issuer"] ?? "algoduck";
        var jwtAudience = jwtConfig["Audience"] ?? "algoduck-client";

        var validateIssuer = jwtConfig.GetValue("ValidateIssuer", environment.IsProduction());
        var validateAudience = jwtConfig.GetValue("ValidateAudience", environment.IsProduction());
        var validateLifetime = jwtConfig.GetValue("ValidateLifetime", true);
        var clockSkewSeconds = jwtConfig.GetValue("ClockSkewSeconds", 60);

        var jwtCookieName =
            jwtConfig.GetValue<string>("JwtCookieName") ??
            jwtConfig.GetValue<string>("AccessTokenCookieName") ??
            "jwt";

        services.AddAuthorization();

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;

                if (environment.IsProduction())
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

        services.AddAuthentication(options =>
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
                        if (context.Request.Cookies.TryGetValue(jwtCookieName, out var token) && !string.IsNullOrWhiteSpace(token))
                        {
                            context.Token = token;
                            return Task.CompletedTask;
                        }

                        var accessToken = context.Request.Query["access_token"].ToString();
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.Headers["X-Token-Expired"] = "true";
                            context.Response.Headers["X-Auth-Error"] = "token_expired";
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        var google = configuration.GetSection("Authentication:Google");
        if (!string.IsNullOrWhiteSpace(google["ClientId"]) || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AUTHENTICATION__GOOGLE__CLIENTID")))
        {
            services.AddAuthentication().AddGoogle("Google", o =>
            {
                o.ClientId = Req(google["ClientId"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__GOOGLE__CLIENTID"), "Authentication:Google:ClientId or AUTHENTICATION__GOOGLE__CLIENTID");
                o.ClientSecret = Req(google["ClientSecret"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__GOOGLE__CLIENTSECRET"), "Authentication:Google:ClientSecret or AUTHENTICATION__GOOGLE__CLIENTSECRET");
                o.CallbackPath = google["CallbackPath"] ?? "/api/auth/oauth/google";
                o.SaveTokens = true;
                o.CorrelationCookie.SameSite = SameSiteMode.Lax;
                if (environment.IsProduction()) o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                o.SignInScheme = IdentityConstants.ExternalScheme;
            });
        }

        var github = configuration.GetSection("Authentication:GitHub");
        if (!string.IsNullOrWhiteSpace(github["ClientId"]) || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AUTHENTICATION__GITHUB__CLIENTID")))
        {
            services.AddAuthentication().AddOAuth("GitHub", o =>
            {
                o.ClientId = Req(github["ClientId"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__GITHUB__CLIENTID"), "Authentication:GitHub:ClientId or AUTHENTICATION__GITHUB__CLIENTID");
                o.ClientSecret = Req(github["ClientSecret"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__GITHUB__CLIENTSECRET"), "Authentication:GitHub:ClientSecret or AUTHENTICATION__GITHUB__CLIENTSECRET");
                o.CallbackPath = github["CallbackPath"] ?? "/api/auth/oauth/github";
                o.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                o.TokenEndpoint = "https://github.com/login/oauth/access_token";
                o.UserInformationEndpoint = "https://api.github.com/user";
                o.Scope.Add("read:user");
                o.Scope.Add("user:email");
                o.SaveTokens = true;
                o.CorrelationCookie.SameSite = SameSiteMode.Lax;
                if (environment.IsProduction()) o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
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

                        var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();

                        using var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                        context.RunClaimActions(user.RootElement);
                    }
                };
            });
        }

        var facebook = configuration.GetSection("Authentication:Facebook");
        if (!string.IsNullOrWhiteSpace(facebook["AppId"]) || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AUTHENTICATION__FACEBOOK__APPID")))
        {
            services.AddAuthentication().AddFacebook("Facebook", o =>
            {
                o.AppId = Req(facebook["AppId"] ?? facebook["ClientId"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__FACEBOOK__APPID"), "Authentication:Facebook:AppId/ClientId or AUTHENTICATION__FACEBOOK__APPID");
                o.AppSecret = Req(facebook["AppSecret"] ?? facebook["ClientSecret"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__FACEBOOK__APPSECRET"), "Authentication:Facebook:AppSecret/ClientSecret or AUTHENTICATION__FACEBOOK__APPSECRET");
                o.CallbackPath = facebook["CallbackPath"] ?? "/api/auth/oauth/facebook";
                o.SaveTokens = true;
                o.Scope.Add("email");
                o.Fields.Add("email");
                o.Fields.Add("name");
                o.CorrelationCookie.SameSite = SameSiteMode.Lax;
                if (environment.IsProduction()) o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                o.SignInScheme = IdentityConstants.ExternalScheme;
            });
        }

        services.AddScoped<JwtTokenProvider>();
        services.AddScoped<TokenParser>();
        services.AddScoped<TokenGenerator>();
        services.AddScoped<TokenRefreshService>();

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

        if (environment.IsDevelopment())
        {
            services.AddScoped<IExternalAuthProvider, DevExternalAuthProvider>();
        }

        services.AddAuthCommands();
        services.AddAuthQueries();

        return services;
    }

    public static void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthModule(builder.Configuration, builder.Environment);
    }
}
