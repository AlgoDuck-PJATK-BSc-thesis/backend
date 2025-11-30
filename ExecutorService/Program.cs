using ExecutorService.Errors;
using ExecutorService.Executor;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.VmLaunchSystem;
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

builder.Services.AddScoped<ICodeExecutorService, CodeExecutorService>();

// eager initialization
var filesystemPooler = await FilesystemPooler.CreateFileSystemPoolerAsync();
var vmLauncher = new VmLaunchManager(filesystemPooler);
var compilationHandler = await CompilationHandler.CreateAsync(vmLauncher);

builder.Services.AddSingleton<ICompilationHandler>(compilationHandler);
builder.Services.AddSingleton<IFilesystemPooler>(filesystemPooler);
builder.Services.AddSingleton(vmLauncher);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowWebApp");

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run("http://0.0.0.0:1337");