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

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var env = builder.Environment;

var jwtConfig = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtConfig["Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
var jwtIssuer = jwtConfig["Issuer"] ?? "algoduck";
var jwtAudience = jwtConfig["Audience"] ?? "algoduck-client";

var validateIssuer = jwtConfig.GetValue<bool>("ValidateIssuer", env.IsProduction());
var validateAudience = jwtConfig.GetValue<bool>("ValidateAudience", env.IsProduction());
var validateLifetime = jwtConfig.GetValue<bool>("ValidateLifetime", true);
var clockSkewSeconds = jwtConfig.GetValue<int>("ClockSkewSeconds", 60);

var jwtCookieName = jwtConfig.GetValue<string>("JwtCookieName", "jwt");

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
        }
        else
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
        }
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ValidateIssuer   = validateIssuer,
            ValidateAudience = validateAudience,
            ValidateLifetime = validateLifetime,

            ValidIssuer   = jwtIssuer,
            ValidAudience = jwtAudience,

            ClockSkew = TimeSpan.FromSeconds(clockSkewSeconds),
            
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies[jwtCookieName];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

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
        Description = "Please provide JWT token in 'Bearer {token}' format"    
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (env.IsProduction())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseHttpsRedirection();
app.UseMiddleware<ErrorHandler>();
app.UseCors(env.IsDevelopment() ? "DevCors" : "ProdCors");

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
await SeedUserRoles(app.Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>());

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

async Task SeedUserRoles(ApplicationDbContext db)
{
    if (!await db.UserRoles.AnyAsync(r => r.Name == "user"))
    {
        db.UserRoles.Add(new UserRole { UserRoleId = Guid.NewGuid(), Name = "user" });
    }
    if (!await db.UserRoles.AnyAsync(r => r.Name == "admin"))
    {
        db.UserRoles.Add(new UserRole { UserRoleId = Guid.NewGuid(), Name = "admin" });
    }
    await db.SaveChangesAsync();
}