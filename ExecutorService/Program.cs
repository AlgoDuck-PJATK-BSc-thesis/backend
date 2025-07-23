using Amazon;
using Amazon.S3;
using ExecutorService.Errors;
using ExecutorService.Executor;
using ExecutorService.Executor.Configs;
using Microsoft.Extensions.Options;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<S3Settings>(builder.Configuration.GetSection("S3Settings"));
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var s3Settings = sp.GetRequiredService<IOptions<S3Settings>>().Value;
    var config = new AmazonS3Config
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(s3Settings.Region)
    };
    return new AmazonS3Client(config);
});

builder.Services.AddScoped<IExecutorRepository, ExecutorRepository>();
builder.Services.AddScoped<ICodeExecutorService, CodeExecutorService>();
builder.Services.AddSingleton<ICompilationHandler, CompilationHandler>();

var app = builder.Build();


app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

// TODO temporary, change this
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapControllers();

app.MapGet("/", async (IOptions<S3Settings> options) =>
{
    return Results.Ok($"{options.Value.BucketName}");
});

app.Run("http://0.0.0.0:1337");