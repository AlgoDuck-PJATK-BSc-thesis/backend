using AlgoDuck.DAL;
using AlgoDuck.Modules.Auth.Commands.Email.VerifyEmail;
using AlgoDuck.Modules.Auth.Shared.Middleware;
using AlgoDuck.Modules.Auth.Shared.Utils;
using AlgoDuck.Modules.Cohort.Shared.Hubs;
using AlgoDuck.Modules.Cohort.Shared.Services;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Modules.Item.Utils;
using AlgoDuck.Modules.Problem.Commands.CreateProblem;
using AlgoDuck.Modules.Problem.Commands.QueryAssistant;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Modules.User.Shared.Utils;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.Middleware;
using AlgoDuck.Shared.Utilities;
using AlgoDuck.Shared.Utilities.DependencyInitializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
    .SetApplicationName("AlgoDuck");

builder.Services.Configure<HostOptions>(o =>
{
    o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.AddHostedService<EmptyCohortCleanupService>();

// general top level stuff
GeneralDependencyInitializer.Initialize(builder);

// app wide configuration
CorsDependencyInitializer.Initialize(builder);
DbDependencyInitializer.Initialize(builder);
RateLimiterDependencyInitializer.Initialize(builder);
SwaggerDependencyInitializer.Initialize(builder);

// module specific dependencies
UserDependencyInitializer.Initialize(builder);
CohortDependencyInitializer.Initialize(builder);
ProblemDependencyInitializer.Initialize(builder);
ItemDependencyInitializer.Initialize(builder);
AuthDependencyInitializer.Initialize(builder);

builder.Services.AddScoped<StandardApiResponseResultFilter>();

builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.Filters.AddService<StandardApiResponseResultFilter>();
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kvp => kvp.Value is not null && kvp.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage)
                    .ToArray()
            );

        return new BadRequestObjectResult(new StandardApiResponse<object?>
        {
            Status = Status.Error,
            Message = "Validation failed.",
            Body = errors
        });
    };
});

var app = builder.Build();

app.UseForwardedHeaders();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (builder.Environment.IsProduction() && Environment.GetEnvironmentVariable("ENABLE_TLS") == "true")
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseMiddleware<SecurityHeaders>();
app.UseMiddleware<ErrorHandler>();
app.UseMiddleware<AuthExceptionMiddleware>();

app.UseStatusCodePages(async statusContext =>
{
    var http = statusContext.HttpContext;
    var path = http.Request.Path;

    if (!path.StartsWithSegments("/api"))
    {
        return;
    }

    if (http.Response.HasStarted)
    {
        return;
    }

    http.Response.ContentType = "application/json; charset=utf-8";

    var statusCode = http.Response.StatusCode;

    var message = statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad request.",
        StatusCodes.Status401Unauthorized => "Unauthorized.",
        StatusCodes.Status403Forbidden => "Forbidden.",
        StatusCodes.Status404NotFound => "Not found.",
        StatusCodes.Status405MethodNotAllowed => "Method not allowed.",
        StatusCodes.Status409Conflict => "Conflict.",
        StatusCodes.Status415UnsupportedMediaType => "Unsupported media type.",
        StatusCodes.Status422UnprocessableEntity => "Validation failed.",
        StatusCodes.Status429TooManyRequests => "Too many requests.",
        StatusCodes.Status500InternalServerError => "Unexpected error.",
        _ => "Request failed."
    };

    await http.Response.WriteAsJsonAsync(new StandardApiResponse
    {
        Status = Status.Error,
        Message = message,
        Body = null
    });
});

app.UseCors(builder.Environment.IsDevelopment() ? "DevCors" : "ProdCors");

app.UseMiddleware<CsrfGuard>();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapVerifyEmailEndpoint();
app.MapControllers();

app.MapHub<CohortChatHub>("/hubs/cohort-chat");
app.MapHub<AssistantHub>("/api/hubs/assistant");
app.MapHub<ExecutionStatusHub>("/api/hubs/execution-status");
app.MapHub<CreateProblemUpdatesHub>("/api/hubs/validation-status");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationCommandDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeedingService>();
    
    
    var attempts = 0;
    const int maxAttempts = 10;
    while (true)
    {
        try
        {
            db.Database.SetCommandTimeout(60);
            db.Database.Migrate();
            await seeder.SeedDataAsync();
            break;
        }
        catch (System.Net.Sockets.SocketException) when (attempts++ < maxAttempts)
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex) when (attempts++ < maxAttempts)
        {
            Console.WriteLine($"DB not ready yet, retry {attempts}/{maxAttempts}: {ex.Message}");
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}

app.Run();

