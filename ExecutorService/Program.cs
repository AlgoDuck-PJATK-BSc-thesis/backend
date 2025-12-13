using AlgoDuckShared;
using ExecutorService.Errors;
using ExecutorService.Executor;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.VmLaunchSystem;
using RabbitMQ.Client;
using CompilationHandler = ExecutorService.Executor.ResourceHandlers.CompilationHandler;

var builder = WebApplication.CreateBuilder(args);

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

builder.Configuration.AddEnvironmentVariables();

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


// eager initialization

var filesystemPooler = await FilesystemPooler.CreateFileSystemPoolerAsync();
var vmLauncher = new VmLaunchManager(filesystemPooler);
var compilationHandler = await CompilationHandler.CreateAsync(vmLauncher);
builder.Services.AddSingleton<ICompilationHandler>(compilationHandler);
builder.Services.AddSingleton<IFilesystemPooler>(filesystemPooler);
builder.Services.AddSingleton(vmLauncher);

builder.Services.AddHostedService<CodeExecutorService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowWebApp");

app.Run();