using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Amazon;
using Amazon.S3;
using DotNetEnv;
using Microsoft.Extensions.Options;
using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Models;
using AlgoDuck.Modules.User.Interfaces;
using AlgoDuck.Modules.User.Services;
using AlgoDuck.Modules.Auth.Jwt;
using AlgoDuck.Modules.Auth.Interfaces;
using AlgoDuck.Modules.Auth.Services;
using AlgoDuck.Modules.Item.Repositories;
using AlgoDuck.Modules.Item.Services;
using AlgoDuck.Modules.Problem.Interfaces;
using AlgoDuck.Modules.Problem.Repositories;
using AlgoDuck.Modules.Cohort.Interfaces;
using AlgoDuck.Modules.Cohort.Services;
using AlgoDuck.Modules.Cohort;
using AlgoDuck.Modules.Problem.Services;
using AlgoDuck.Shared.Configs;
using AlgoDuck.Shared.Utilities;
using Microsoft.OpenApi.Models;

Env.Load(); 

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = new JwtSettings
{
    Key = Environment.GetEnvironmentVariable("JWT_KEY"),
    Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
    Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
    DurationInMinutes = double.Parse(Environment.GetEnvironmentVariable("JWT_EXP_MINUTES") ?? "120")
};

builder.Services.Configure<JwtSettings>(opts =>
{
    opts.Key = jwtSettings.Key;
    opts.Issuer = jwtSettings.Issuer;
    opts.Audience = jwtSettings.Audience;
    opts.DurationInMinutes = jwtSettings.DurationInMinutes;
});

var connectionString =
    $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
    $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
    $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
    $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
    $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["jwt"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.Configure<S3Settings>(builder.Configuration.GetSection("S3Settings"));
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var s3Settings = sp.GetRequiredService<IOptions<S3Settings>>().Value;

    var credentials = new Amazon.Runtime.BasicAWSCredentials(
        Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
        Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
    );

    var config = new AmazonS3Config
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(s3Settings.Region)
    };

    return new AmazonS3Client(credentials, config);
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IExecutorService, CodeExecutorService>();
builder.Services.AddScoped<ICohortService, CohortService>();
builder.Services.AddScoped<ICohortChatService, CohortChatService>();
builder.Services.AddScoped<ICohortLeaderboardService, CohortLeaderboardService>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IProblemService, ProblemService>();
builder.Services.AddScoped<IProblemRepository, ProblemRepository>();
builder.Services.AddScoped<DataSeedingService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
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
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AlgoDuck API",
        Version = "v1"
    });
});

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<AlgoDuck.Shared.Middleware.ErrorHandler>();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<CohortChatHub>("/hubs/cohort-chat");

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

    string[] roles = { "admin", "user" };

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
        db.UserRoles.Add(new UserRole
        {
            UserRoleId = Guid.NewGuid(),
            Name = "user"
        });
    }

    if (!await db.UserRoles.AnyAsync(r => r.Name == "admin"))
    {
        db.UserRoles.Add(new UserRole
        {
            UserRoleId = Guid.NewGuid(),
            Name = "admin"
        });
    }

    await db.SaveChangesAsync();
}
