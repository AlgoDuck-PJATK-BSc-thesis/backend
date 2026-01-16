using System.Text.Json;
using AlgoDuckShared;
using ExecutorService.Errors;
using ExecutorService.Executor;
using ExecutorService.Executor.BackgroundWorkers;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.Types;
using ExecutorService.Executor.Types.Config;
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

builder.Services.Configure<HealthCheckConfig>(
    builder.Configuration.GetSection("HealthCheck"));
builder.Services.Configure<CompilationHandlerConfig>(
    builder.Configuration.GetSection("CompilerManager"));
builder.Services.Configure<FileSystemPoolerConfig>(
    builder.Configuration.GetSection("PoolerConfig"));


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

builder.Services.AddSingleton<FilesystemPooler>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<FilesystemPooler>>();
    var config = sp.GetRequiredService<IOptions<FileSystemPoolerConfig>>();
    return FilesystemPooler.CreateFileSystemPoolerAsync(logger, config).GetAwaiter().GetResult();
});

builder.Services.AddSingleton<VmLaunchManager>(sp =>
{
    var pooler = sp.GetRequiredService<FilesystemPooler>();
    var logger = sp.GetRequiredService<ILogger<VmLaunchManager>>();
    var config = sp.GetRequiredService<IOptions<ExecutorConfiguration>>();
    var healthCheckConfig = sp.GetRequiredService<IOptions<HealthCheckConfig>>();

    return new VmLaunchManager(pooler, logger, config, healthCheckConfig);
});

builder.Services.AddSingleton<ICompilationHandler>(sp =>
{
    var launchManager = sp.GetRequiredService<VmLaunchManager>();
    var options = sp.GetRequiredService<IOptions<CompilationHandlerConfig>>();
    var logger = sp.GetRequiredService<ILogger<CompilationHandler>>();
    return CompilationHandler.CreateAsync(launchManager, options, logger).GetAwaiter().GetResult();
});

builder.Services.AddHostedService<ExecutorBackgroundWorker>(sp =>
{
    var rabbitMqConnectionService = sp.GetRequiredService<IRabbitMqConnectionService>();
    var manager = sp.GetRequiredService<VmLaunchManager>();
    var logger = sp.GetRequiredService<ILogger<ExecutorBackgroundWorker>>();
    var handler = sp.GetRequiredService<ICompilationHandler>();
    return new ExecutorBackgroundWorker(handler, manager, rabbitMqConnectionService, logger, new ServiceData()
    {
        ServiceName = "Executor",
        RequestQueueName = "code_execution_requests",
        ResponseQueueName = "code_execution_results"
    });
});

builder.Services.AddHostedService<ValidatorBackgroundWorker>(sp =>
{
    var rabbitMqConnectionService = sp.GetRequiredService<IRabbitMqConnectionService>();
    var manager = sp.GetRequiredService<VmLaunchManager>();
    var logger = sp.GetRequiredService<ILogger<ValidatorBackgroundWorker>>();
    var handler = sp.GetRequiredService<ICompilationHandler>();
    return new ValidatorBackgroundWorker(handler, manager, rabbitMqConnectionService, logger, new ServiceData()
    {
        ServiceName = "Validator",
        RequestQueueName = "problem_validation_requests",
        ResponseQueueName = "problem_validation_results"
    });
});

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