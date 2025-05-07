using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
using WebApplication1.DAL;
using WebApplication1.Modules.UserModule.Models;
using WebApplication1.Modules.AuthModule.Jwt;
using WebApplication1.Modules.AuthModule.Interfaces;
using WebApplication1.Modules.AuthModule.Services;
using WebApplication1.Modules.UserModule.Interfaces;
using WebApplication1.Modules.UserModule.Services;

Env.Load(); 

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = new JwtSettings
{
    Key = Env.GetString("JWT_KEY"),
    Issuer = Env.GetString("JWT_ISSUER"),
    Audience = Env.GetString("JWT_AUDIENCE"),
    DurationInMinutes = double.Parse(Env.GetString("JWT_EXP_MINUTES") ?? "120")
};

builder.Services.Configure<JwtSettings>(opts =>
{
    opts.Key = jwtSettings.Key;
    opts.Issuer = jwtSettings.Issuer;
    opts.Audience = jwtSettings.Audience;
    opts.DurationInMinutes = jwtSettings.DurationInMinutes;
});

var connectionString =
    $"Host={Env.GetString("DB_HOST")};" +
    $"Port={Env.GetString("DB_PORT")};" +
    $"Database={Env.GetString("DB_NAME")};" +
    $"Username={Env.GetString("DB_USER")};" +
    $"Password={Env.GetString("DB_PASSWORD")}";

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

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

await SeedRoles(app.Services);

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