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
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SharedEmailSender = AlgoDuck.Modules.Auth.Shared.Interfaces.IEmailSender;
using SharedEmailTransport = AlgoDuck.Modules.Auth.Shared.Interfaces.IEmailTransport;
using SharedPostmarkEmailSender = AlgoDuck.Modules.Auth.Shared.Email.PostmarkEmailSender;
using SharedGmailSmtpEmailSender = AlgoDuck.Modules.Auth.Shared.Email.GmailSmtpEmailSender;
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

    private const string DevFallbackSigningKey = "dev_signing_key_dev_signing_key_dev_signing_key_32+";

    private static string FrontendBaseUrl(IConfiguration configuration)
    {
        var raw = configuration["App:FrontendUrl"];
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "http://localhost:5173";
        }
        return raw.Trim().TrimEnd('/');
    }

    private static string BuildOauthErrorRedirect(IConfiguration configuration, string providerKey, string reason)
    {
        var baseUrl = FrontendBaseUrl(configuration);
        return $"{baseUrl}/login?oauthError={Uri.EscapeDataString(reason)}&provider={Uri.EscapeDataString(providerKey)}";
    }

    private static Task RedirectRemoteFailure(RemoteFailureContext ctx, IConfiguration configuration, string providerKey)
    {
        ctx.HandleResponse();

        var error =
            (ctx.Request.Query["error"].ToString()).Trim();

        var reason =
            error.Equals("access_denied", StringComparison.OrdinalIgnoreCase)
                ? "access_denied"
                : "oauth_failed";

        ctx.Response.Redirect(BuildOauthErrorRedirect(configuration, providerKey, reason));
        return Task.CompletedTask;
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

                if (string.IsNullOrWhiteSpace(s.SigningKey) && !environment.IsProduction())
                {
                    s.SigningKey = DevFallbackSigningKey;
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

        if (string.IsNullOrWhiteSpace(jwtKey) && !environment.IsProduction())
        {
            jwtKey = DevFallbackSigningKey;
        }

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
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                {
                    KeyId = "algoduck-symmetric-v1"
                };

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
                    },
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json; charset=utf-8";

                        var msg = context.Response.Headers.TryGetValue("X-Token-Expired", out var expired) && expired == "true"
                            ? "Access token expired."
                            : "Unauthorized.";

                        await context.Response.WriteAsJsonAsync(new StandardApiResponse
                        {
                            Status = Status.Error,
                            Message = msg,
                        });
                    },
                    OnForbidden = async context =>
                    {
                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json; charset=utf-8";

                        await context.Response.WriteAsJsonAsync(new StandardApiResponse
                        {
                            Status = Status.Error,
                            Message = "Forbidden.",
                        });
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

                o.Events = new OAuthEvents
                {
                    OnRemoteFailure = ctx => RedirectRemoteFailure(ctx, configuration, "google")
                };
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
                    OnRemoteFailure = ctx => RedirectRemoteFailure(ctx, configuration, "github"),
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

                        var existingEmail = context.Identity?.FindFirst(ClaimTypes.Email)?.Value;
                        if (string.IsNullOrWhiteSpace(existingEmail))
                        {
                            var emailsRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
                            emailsRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            emailsRequest.Headers.UserAgent.ParseAdd("AlgoDuckOAuth");
                            emailsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                            var emailsResponse = await context.Backchannel.SendAsync(emailsRequest, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                            emailsResponse.EnsureSuccessStatusCode();

                            using var emailsDoc = JsonDocument.Parse(await emailsResponse.Content.ReadAsStringAsync());

                            string? bestEmail = null;
                            var bestScore = -1;

                            if (emailsDoc.RootElement.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var el in emailsDoc.RootElement.EnumerateArray())
                                {
                                    if (!el.TryGetProperty("email", out var emailProp)) continue;
                                    var email = emailProp.GetString();
                                    if (string.IsNullOrWhiteSpace(email)) continue;

                                    var verified = el.TryGetProperty("verified", out var verifiedProp) && verifiedProp.ValueKind == JsonValueKind.True;
                                    var primary = el.TryGetProperty("primary", out var primaryProp) && primaryProp.ValueKind == JsonValueKind.True;

                                    var score = 0;
                                    if (verified) score += 2;
                                    if (primary) score += 1;

                                    if (score > bestScore)
                                    {
                                        bestScore = score;
                                        bestEmail = email;
                                    }

                                    if (bestScore == 3)
                                    {
                                        break;
                                    }
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(bestEmail) && context.Identity is not null)
                            {
                                context.Identity.AddClaim(new Claim(ClaimTypes.Email, bestEmail));
                            }
                        }
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

                o.Events = new OAuthEvents
                {
                    OnRemoteFailure = ctx => RedirectRemoteFailure(ctx, configuration, "facebook")
                };
            });
        }

        var microsoft = configuration.GetSection("Authentication:Microsoft");
        if (!string.IsNullOrWhiteSpace(microsoft["ClientId"]) || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AUTHENTICATION__MICROSOFT__CLIENTID")))
        {
            services.AddAuthentication().AddOpenIdConnect("Microsoft", o =>
            {
                o.ClientId = Req(microsoft["ClientId"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__MICROSOFT__CLIENTID"), "Authentication:Microsoft:ClientId or AUTHENTICATION__MICROSOFT__CLIENTID");
                o.ClientSecret = Req(microsoft["ClientSecret"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__MICROSOFT__CLIENTSECRET"), "Authentication:Microsoft:ClientSecret or AUTHENTICATION__MICROSOFT__CLIENTSECRET");
                o.Authority = microsoft["Authority"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__MICROSOFT__AUTHORITY") ?? "https://login.microsoftonline.com/common/v2.0";
                o.CallbackPath = microsoft["CallbackPath"] ?? "/api/auth/oauth/microsoft";
                o.ResponseType = OpenIdConnectResponseType.Code;
                o.ResponseMode = OpenIdConnectResponseMode.Query;
                o.UsePkce = true;
                o.SaveTokens = true;
                o.GetClaimsFromUserInfoEndpoint = true;
                o.Scope.Add("email");
                o.SignInScheme = IdentityConstants.ExternalScheme;

                o.CorrelationCookie.SameSite = SameSiteMode.Lax;
                o.NonceCookie.SameSite = SameSiteMode.Lax;

                if (environment.IsProduction())
                {
                    o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                    o.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
                }

                var authority = o.Authority ?? string.Empty;
                var isMultiTenant =
                    authority.Contains("/common", StringComparison.OrdinalIgnoreCase) ||
                    authority.Contains("/organizations", StringComparison.OrdinalIgnoreCase) ||
                    authority.Contains("/consumers", StringComparison.OrdinalIgnoreCase);

                if (isMultiTenant)
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidAudience = o.ClientId,
                        ValidateLifetime = true,
                        IssuerValidator = (issuer, _, _) =>
                        {
                            if (string.IsNullOrWhiteSpace(issuer))
                            {
                                throw new SecurityTokenInvalidIssuerException("Issuer is missing.");
                            }

                            if (issuer.StartsWith("https://login.microsoftonline.com/", StringComparison.OrdinalIgnoreCase) &&
                                issuer.EndsWith("/v2.0", StringComparison.OrdinalIgnoreCase))
                            {
                                return issuer;
                            }

                            throw new SecurityTokenInvalidIssuerException($"Invalid issuer: {issuer}");
                        }
                    };
                }

                o.Events = new OpenIdConnectEvents
                {
                    OnAuthorizationCodeReceived = ctx =>
                    {
                        Console.WriteLine($"[MS OAUTH] Authorization code received");
                        return Task.CompletedTask;
                    },
                    OnTokenResponseReceived = ctx =>
                    {
                        Console.WriteLine($"[MS OAUTH] Token response received");
                        return Task.CompletedTask;
                    },
                    OnRemoteFailure = ctx =>
                    {
                        Console.WriteLine($"[MS OAUTH ERROR] {ctx.Failure?.Message}");
                        Console.WriteLine($"[MS OAUTH ERROR] Inner: {ctx.Failure?.InnerException?.Message}");
                        return RedirectRemoteFailure(ctx, configuration, "microsoft");
                    },
                    OnRedirectToIdentityProvider = context =>
                    {
                        if (context.Properties.Items.TryGetValue("prompt", out var prompt))
                        {
                            var p = (prompt ?? string.Empty).Trim();
                            if (p == "select_account" || p == "login")
                            {
                                context.ProtocolMessage.Prompt = p;
                            }
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var identity = context.Principal?.Identity as ClaimsIdentity;
                        if (identity is null)
                        {
                            return Task.CompletedTask;
                        }

                        var oid = identity.FindFirst("oid")?.Value;
                        var sub = identity.FindFirst("sub")?.Value;

                        if (!string.IsNullOrWhiteSpace(oid))
                        {
                            var existing = identity.FindFirst(ClaimTypes.NameIdentifier);
                            if (existing is null || (!string.IsNullOrWhiteSpace(sub) && existing.Value == sub))
                            {
                                if (existing is not null)
                                {
                                    identity.RemoveClaim(existing);
                                }
                                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, oid));
                            }
                        }

                        var email =
                            identity.FindFirst(ClaimTypes.Email)?.Value ??
                            identity.FindFirst("email")?.Value ??
                            identity.FindFirst("preferred_username")?.Value ??
                            identity.FindFirst("upn")?.Value ??
                            string.Empty;

                        if (!string.IsNullOrWhiteSpace(email) && identity.FindFirst(ClaimTypes.Email) is null)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Email, email));
                        }

                        var name =
                            identity.FindFirst(ClaimTypes.Name)?.Value ??
                            identity.FindFirst("name")?.Value ??
                            string.Empty;

                        if (string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(email) && identity.FindFirst(ClaimTypes.Name) is null)
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Name, email));
                        }

                        return Task.CompletedTask;
                    }
                };
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

        var emailProvider =
            (Environment.GetEnvironmentVariable("EMAIL__PROVIDER") ?? configuration["Email:Provider"] ?? string.Empty)
            .Trim();

        var wantsSmtp = emailProvider.Equals("smtp", StringComparison.OrdinalIgnoreCase)
            || emailProvider.Equals("gmail", StringComparison.OrdinalIgnoreCase)
            || emailProvider.Equals("gmailsmtp", StringComparison.OrdinalIgnoreCase)
            || emailProvider.Equals("gmail_smtp", StringComparison.OrdinalIgnoreCase);

        var hasGmailSmtp = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GMAIL__SMTP_EMAIL"))
            && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GMAIL__SMTP_PASSWORD"));

        if (wantsSmtp || (string.IsNullOrWhiteSpace(emailProvider) && hasGmailSmtp))
        {
            if (!hasGmailSmtp)
            {
                throw new InvalidOperationException("EMAIL__PROVIDER is set to SMTP/Gmail, but GMAIL__SMTP_EMAIL or GMAIL__SMTP_PASSWORD is missing.");
            }

            services.AddScoped<SharedEmailTransport, SharedGmailSmtpEmailSender>();
        }
        else
        {
            services.AddScoped<SharedEmailTransport, SharedPostmarkEmailSender>();
        }

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
