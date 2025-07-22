using ExecutorService.Errors;
using ExecutorService.Executor;
using ExecutorService.Executor.Configs;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IExecutorRepository, ExecutorRepositoryMock>();
builder.Services.AddScoped<ICodeExecutorService, CodeExecutorService>();
builder.Services.AddSingleton<ICompilationHandler, CompilationHandler>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();

app.MapControllers();

app.Run("http://0.0.0.0:1337");