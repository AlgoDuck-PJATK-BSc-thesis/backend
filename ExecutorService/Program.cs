using AlgoDuckShared;
using ExecutorService.Errors;
using ExecutorService.Executor;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.Types.VmLaunchTypes;
using ExecutorService.Executor.VmLaunchSystem;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using CompilationHandler = ExecutorService.Executor.ResourceHandlers.CompilationHandler;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddExecutorConfiguration(builder.Configuration);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("http://localhost:8080")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new ConnectionFactory
    {
        HostName = configuration["RabbitMq:HostName"] ?? "localhost",
        UserName = configuration["RabbitMq:UserName"] ?? "guest",
        Password = configuration["RabbitMq:Password"] ?? "guest",
        Port = configuration.GetValue("RabbitMq:Port", 5672),
        AutomaticRecoveryEnabled = true,
        NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
        RequestedConnectionTimeout = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddSingleton<IRabbitMqConnectionService, RabbitMqConnectionService>();

builder.Services.AddSingleton<FilesystemPooler>(sp => FilesystemPooler.CreateFileSystemPoolerAsync().GetAwaiter().GetResult());

builder.Services.AddSingleton<VmLaunchManager>(sp =>
{
    var pooler = sp.GetRequiredService<FilesystemPooler>();
    var logger = sp.GetRequiredService<ILogger<VmLaunchManager>>();
    var config = sp.GetRequiredService<IOptions<ExecutorConfiguration>>();
    return new VmLaunchManager(pooler, logger, config);
});

builder.Services.AddSingleton<ICompilationHandler>(sp =>
{
    var launchManager = sp.GetRequiredService<VmLaunchManager>();
    return CompilationHandler.CreateAsync(launchManager).GetAwaiter().GetResult();
});

builder.Services.AddSingleton<IFilesystemPooler>(sp => sp.GetRequiredService<FilesystemPooler>());

builder.Services.AddHostedService<CodeExecutorService>();

var app = builder.Build();

app.UseExceptionHandling();

app.UseCors("AllowWebApp");
app.UseRouting();


app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("ExecutorService starting");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);

await app.RunAsync();