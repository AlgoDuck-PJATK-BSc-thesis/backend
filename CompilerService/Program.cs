using System.Diagnostics;
using CompilerService.Dtos;
using Microsoft.AspNetCore.Mvc;
using CompilationErrorDto = CompilerService.Dtos.CompilationErrorDto;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/compile", ([FromBody] CompileRequestDto requestDto) =>
{
    var compileId = Guid.NewGuid().ToString();

    var compileProcess = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"/app/scripts/compile-src.sh {requestDto.ClassName} {requestDto.SrcCodeB64} {compileId}",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            CreateNoWindow = true
        }
    };
    
    compileProcess.Start();
    compileProcess.WaitForExit();
    try
    {
        return Results.File(File.ReadAllBytes($"/app/client-bytecode/{compileId}/{requestDto.ClassName}.class"),
            "application/octet-stream");
    }
    catch (Exception e)
    {
        if (e is FileNotFoundException or DirectoryNotFoundException)
        {
            var errorLog = File.ReadAllText($"/app/error-log/{compileId}/err.log");
            var cleanedErrorLog = errorLog.Replace($"/app/client-src/{compileId}/", "");
            return Results.BadRequest(new CompilationErrorDto(cleanedErrorLog));
        }
        return Results.Problem(
            title: "Something went wrong during code execution. Please try again later",
            detail: e.Message,
            statusCode: StatusCodes.Status500InternalServerError
        );
    }
});

app.Run("http://0.0.0.0:5137");