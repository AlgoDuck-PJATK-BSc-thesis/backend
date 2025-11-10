using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Text;
using Amazon;
using Amazon.S3;
using AlgoDuck.DAL;
using AlgoDuck.Models.User;
using AlgoDuck.Modules.User.Interfaces;
using AlgoDuck.Modules.User.Services;
using AlgoDuck.Modules.Auth.Jwt;
using AlgoDuck.Modules.Auth.Interfaces;
using AlgoDuck.Modules.Auth.Services;
using AlgoDuck.Modules.Item.Repositories;
using AlgoDuck.Modules.Item.Services;
using AlgoDuck.Modules.Cohort.Interfaces;
using AlgoDuck.Modules.Cohort.Services;
using AlgoDuck.Modules.Cohort;
using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuck.Modules.Problem.ExecutorShared;
using AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;
using AlgoDuck.Modules.Problem.Queries.GetProblemDetailsById;
using AlgoDuck.Modules.Problem.Queries.QueryAssistant;
using AlgoDuck.Shared.Utilities;
using AlgoDuckShared;
using OpenAI.Chat;
using S3Settings = AlgoDuck.Shared.Configs.S3Settings;
using AlgoDuck.Modules.Cohort.CohortManagement;
using AlgoDuck.Modules.Cohort.CohortManagement.Shared;
using System.Security.Claims;
using AlgoDuck.Shared.Middleware;
using System.Threading.RateLimiting;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var env = builder.Environment;

var jwtConfig = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtConfig["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
var jwtIssuer = jwtConfig["Issuer"] ?? "algoduck";
var jwtAudience = jwtConfig["Audience"] ?? "algoduck-client";

var validateIssuer = jwtConfig.GetValue("ValidateIssuer", env.IsProduction());
var validateAudience = jwtConfig.GetValue("ValidateAudience", env.IsProduction());
var validateLifetime = jwtConfig.GetValue("ValidateLifetime", true);
var clockSkewSeconds = jwtConfig.GetValue("ClockSkewSeconds", 60);

var jwtCookieName = jwtConfig.GetValue<string>("JwtCookieName") ?? "jwt";

var devOrigins = builder.Configuration.GetSection("Cors:DevOrigins").Get<string[]>() 
                 ?? new[] { "http://localhost:5173", "https://localhost:5173" };
var prodOrigins = builder.Configuration.GetSection("Cors:ProdOrigins").Get<string[]>() ?? Array.Empty<string>();

if (env.IsProduction() && prodOrigins.Length == 0)
    throw new InvalidOperationException("Cors:ProdOrigins must be configured in Production.");

builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .Validate(s => !string.IsNullOrWhiteSpace(s.Key))
    .Validate(s => !string.IsNullOrWhiteSpace(s.Issuer))
    .Validate(s => !string.IsNullOrWhiteSpace(s.Audience))
    .Validate(s => s.DurationInMinutes > 0)
    .ValidateOnStart();

builder.Services.Configure<S3Settings>(builder.Configuration.GetSection("S3Settings"));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

var keysPath = env.IsDevelopment()
    ? Path.Combine(builder.Environment.ContentRootPath, "keys")
    : "/var/app-keys";
Directory.CreateDirectory(keysPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("AlgoDuck");

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.User.RequireUniqueEmail = true;

        if (env.IsProduction())
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
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

static string Req(string? v, string key)
{
    if (string.IsNullOrWhiteSpace(v)) throw new InvalidOperationException($"Missing configuration: {key}");
    return v;
}

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
            if (context.Request.Cookies.TryGetValue(jwtCookieName, out var token) && !string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
})
.AddCookie(IdentityConstants.ExternalScheme, o =>
{
    o.Cookie.Name = "ext_auth";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.Cookie.SecurePolicy = env.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
})
.AddGoogle("Google", o =>
{
    var g = builder.Configuration.GetSection("Authentication:Google");
    o.ClientId = Req(g["ClientId"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__GOOGLE__CLIENTID"), "Authentication:Google:ClientId or AUTHENTICATION__GOOGLE__CLIENTID");
    o.ClientSecret = Req(g["ClientSecret"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__GOOGLE__CLIENTSECRET"), "Authentication:Google:ClientSecret or AUTHENTICATION__GOOGLE__CLIENTSECRET");
    o.CallbackPath = g["CallbackPath"] ?? "/api/auth/oauth/google";
    o.SaveTokens = true;
    o.CorrelationCookie.SameSite = SameSiteMode.Lax;
    if (env.IsProduction()) o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    o.SignInScheme = IdentityConstants.ExternalScheme;
})
.AddOAuth("GitHub", o =>
{
    var gh = builder.Configuration.GetSection("Authentication:GitHub");
    o.ClientId = Req(gh["ClientId"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__GITHUB__CLIENTID"), "Authentication:GitHub:ClientId or AUTHENTICATION__GITHUB__CLIENTID");
    o.ClientSecret = Req(gh["ClientSecret"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__GITHUB__CLIENTSECRET"), "Authentication:GitHub:ClientSecret or AUTHENTICATION__GITHUB__CLIENTSECRET");
    o.CallbackPath = gh["CallbackPath"] ?? "/api/auth/oauth/github";
    o.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
    o.TokenEndpoint = "https://github.com/login/oauth/access_token";
    o.UserInformationEndpoint = "https://api.github.com/user";
    o.Scope.Add("read:user");
    o.Scope.Add("user:email");
    o.SaveTokens = true;
    o.CorrelationCookie.SameSite = SameSiteMode.Lax;
    if (env.IsProduction()) o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
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

            using var user = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            context.RunClaimActions(user.RootElement);

            var email = context.Identity?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
            {
                var emailsReq = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
                emailsReq.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                emailsReq.Headers.UserAgent.ParseAdd("AlgoDuckOAuth");
                emailsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                var emailsRes = await context.Backchannel.SendAsync(
                    emailsReq, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                if (emailsRes.IsSuccessStatusCode)
                {
                    var json = await emailsRes.Content.ReadAsStringAsync();
                    var arr = System.Text.Json.JsonDocument.Parse(json).RootElement;
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
    var appId = Req(fb["AppId"] ?? fb["ClientId"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__FACEBOOK__APPID"), "Authentication:Facebook:AppId/ClientId or AUTHENTICATION__FACEBOOK__APPID");
    var appSecret = Req(fb["AppSecret"] ?? fb["ClientSecret"] ?? Environment.GetEnvironmentVariable("AUTHENTICATION__FACEBOOK__APPSECRET"), "Authentication:Facebook:AppSecret/ClientSecret or AUTHENTICATION__FACEBOOK__APPSECRET");
    o.AppId = appId;
    o.AppSecret = appSecret;
    o.CallbackPath = fb["CallbackPath"] ?? "/api/auth/oauth/facebook";
    o.SaveTokens = true;
    o.Scope.Add("email");
    o.Fields.Add("email");
    o.Fields.Add("name");
    o.CorrelationCookie.SameSite = SameSiteMode.Lax;
    if (env.IsProduction()) o.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    o.SignInScheme = IdentityConstants.ExternalScheme;
});

builder.Services.AddMemoryCache();
builder.Services.Configure<DataProtectionTokenProviderOptions>(o => o.TokenLifespan = TimeSpan.FromMinutes(10));
builder.Services.AddSingleton<AlgoDuck.Modules.Auth.Email.IEmailSender, AlgoDuck.Modules.Auth.Email.PostmarkEmailSender>();
builder.Services.AddSingleton<AlgoDuck.Modules.Auth.TwoFactor.ITwoFactorService, AlgoDuck.Modules.Auth.TwoFactor.TwoFactorService>();

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<ICohortService, CohortService>();
builder.Services.AddScoped<ICohortChatService, CohortChatService>();
builder.Services.AddScoped<ICohortLeaderboardService, CohortLeaderboardService>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemService, ItemService>();

builder.Services.AddScoped<DataSeedingService>();
builder.Services.AddScoped<ICohortRepository, CohortRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins(devOrigins)
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

    options.AddPolicy("ProdCors", policy =>
    {
        policy.WithOrigins(prodOrigins)
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (ctx, token) =>
    {
        ctx.HttpContext.Response.ContentType = "application/json";

        if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            ctx.HttpContext.Response.Headers["Retry-After"] = 
                ((int)retryAfter.TotalSeconds).ToString();
        }

        await ctx.HttpContext.Response.WriteAsJsonAsync(
            ApiResponse.Fail("Too many requests. Please slow down.", "too_many_requests"),
            cancellationToken: token
        );
    };
    
    options.AddPolicy("AuthTight", httpContext =>
    {
        var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: key,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            }
        );
    });
});

var csrfHeaderName = jwtConfig.GetValue<string>("CsrfHeaderName") ?? "X-CSRF-Token";

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AlgoDuck API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Optional: Bearer token (not used by cookie auth)"
    });

    c.AddSecurityDefinition("CsrfToken", new OpenApiSecurityScheme
    {
        Name = csrfHeaderName,
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "CSRF token matching the CSRF cookie"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "CsrfToken" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = env.IsDevelopment();
});

// ExecutorDependencyInitializer.InitializeExecutorDependencies(builder);

builder.Services.AddHttpClient("executor", client =>
{
    client.BaseAddress =
        new Uri($"http://executor:{Environment.GetEnvironmentVariable("EXECUTOR_PORT") ?? "1337"}/api/execute");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
        
builder.Services.AddSingleton<ChatClient>(sp =>
{
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    const string model = "gpt-5-nano";
    return new ChatClient(model, apiKey);
});
        
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var s3 = sp.GetRequiredService<IOptions<S3Settings>>().Value;
    var config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(s3.Region) };
    return new AmazonS3Client(config);
});

if (env.IsDevelopment())
{
    builder.Services.Configure<ForwardedHeadersOptions>(o =>
    {
        o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
        o.KnownNetworks.Clear();
        o.KnownProxies.Clear();
        o.ForwardLimit = 1;
        o.RequireHeaderSymmetry = false;
    });
}
else
{
    builder.Services.Configure<ForwardedHeadersOptions>(o =>
    {
        o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
        o.KnownProxies.Add(IPAddress.Loopback);
        o.KnownProxies.Add(IPAddress.IPv6Loopback);

        o.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("172.17.0.0"), 16));
        o.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("10.0.0.0"), 24));
        o.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("10.244.0.0"), 16));

        o.ForwardLimit = 1;
    });
}

builder.Services.Configure<SecurityHeadersOptions>(builder.Configuration.GetSection("SecurityHeaders"));

if (env.IsProduction())
{
    builder.Services.AddHsts(o =>
    {
        o.Preload = true;
        o.IncludeSubDomains = true;
        o.MaxAge = TimeSpan.FromDays(365);
        o.ExcludedHosts.Add("localhost");
        o.ExcludedHosts.Add("127.0.0.1");
        o.ExcludedHosts.Add("[::1]");
    });
}

builder.Services.AddScoped<IExecutorQueryInterface, ExecutorQueryInterface>();
builder.Services.AddScoped<IExecutorSubmitService, ExecutorSubmitService>();
builder.Services.AddScoped<IExecutorDryService, ExecutorDryService>();
builder.Services.AddScoped<IAwsS3Client, AwsS3Client>();
builder.Services.AddScoped<IProblemRepository, ProblemRepository>();
builder.Services.AddScoped<IProblemService, ProblemService>();
builder.Services.AddScoped<IAssistantService, AssistantService>();


var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (env.IsProduction() && Environment.GetEnvironmentVariable("ENABLE_TLS") == "true")
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseMiddleware<SecurityHeaders>();
app.UseMiddleware<ErrorHandler>();
app.UseCors(env.IsDevelopment() ? "DevCors" : "ProdCors");

app.UseMiddleware<CsrfGuard>();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<CohortChatHub>("/hubs/cohort-chat");
app.MapCohortManagementEndpoints();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
    var seedingService = scope.ServiceProvider.GetRequiredService<DataSeedingService>();
    await seedingService.SeedDataAsync();
}

await SeedRoles(app.Services);

app.Run();

async Task SeedRoles(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    string[] roles = ["admin", "user"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
        }
    }
}
