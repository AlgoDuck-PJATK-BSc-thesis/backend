using AlgoDuckShared;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using ExecutorService.Errors;
using ExecutorService.Executor;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.VmLaunchSystem;
using Microsoft.Extensions.Options;
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

builder.Services.Configure<S3Settings>(builder.Configuration.GetSection("S3Settings"));
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var s3Settings = sp.GetRequiredService<IOptions<S3Settings>>().Value;

    Console.WriteLine($"BuckerName: {s3Settings.BucketName}");
    Console.WriteLine($"BuckerName: {s3Settings.Region}");
    var credentials = new BasicAWSCredentials(
        Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
        Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
    );

    var config = new AmazonS3Config
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(s3Settings.Region)
    };

    return new AmazonS3Client(credentials, config);
});

builder.Services.AddScoped<ICodeExecutorService, CodeExecutorService>();
builder.Services.AddScoped<IAwsS3Client, AwsS3Client>();

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