using AlgoDuck.DAL;
using AlgoDuck.Modules.Auth.Shared.Utils;
using AlgoDuck.Modules.Cohort.Shared.Hubs;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Modules.Item.Utils;
using AlgoDuck.Modules.Problem.Commands.QueryAssistant;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Modules.User.Shared.Utils;
using AlgoDuck.Shared.Middleware;
using AlgoDuck.Shared.Utilities;
using AlgoDuck.Shared.Utilities.DependencyInitializers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
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
app.UseCors(builder.Environment.IsDevelopment() ? "DevCors" : "ProdCors");

app.UseMiddleware<CsrfGuard>();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<CohortChatHub>("/hubs/cohort-chat");
app.MapHub<AssistantHub>("/api/hubs/assistant");
app.MapHub<ExecutionStatusHub>("/api/hubs/execution-status");

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
        catch (System.Net.Sockets.SocketException ex) when (attempts++ < maxAttempts)
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
