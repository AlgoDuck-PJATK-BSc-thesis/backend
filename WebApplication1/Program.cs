using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Amazon;
using Amazon.S3;
using DotNetEnv;
using Microsoft.Extensions.Options;
using WebApplication1.DAL;
using WebApplication1.Modules.UserModule.Models;
using WebApplication1.Modules.UserModule.Interfaces;
using WebApplication1.Modules.UserModule.Services;
using WebApplication1.Modules.AuthModule.Jwt;
using WebApplication1.Modules.AuthModule.Interfaces;
using WebApplication1.Modules.AuthModule.Services;
using WebApplication1.Modules.ItemModule.Repositories;
using WebApplication1.Modules.ItemModule.Services;
using WebApplication1.Modules.ProblemModule.Interfaces;
using WebApplication1.Modules.ProblemModule.Repositories;
using WebApplication1.Modules.CohortModule.Interfaces;
using WebApplication1.Modules.CohortModule.Services;
using WebApplication1.Modules.CohortModule.Chat;
using WebApplication1.Modules.CohortModule.Chat.Interfaces;
using WebApplication1.Modules.CohortModule.Chat.Services;
using WebApplication1.Modules.CohortModule.Leaderboard.Interfaces;
using WebApplication1.Modules.CohortModule.Leaderboard.Services;
using WebApplication1.Modules.ProblemModule.Services;
using WebApplication1.Modules.UserModule.Interfaces;
using WebApplication1.Modules.UserModule.Services;
using WebApplication1.Shared.Configs;
using WebApplication1.Shared.Utilities;

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
builder.Services.AddSwaggerGen();

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
app.UseMiddleware<WebApplication1.Shared.Middleware.ErrorHandler>();
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
