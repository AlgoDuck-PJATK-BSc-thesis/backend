using System.Diagnostics;
using CompilerService.Dtos;
using Microsoft.AspNetCore.Mvc;

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

    var dirPath = $"/app/client-bytecode/{compileId}";
    
    try
    {
        var generatedClassFiles = new Dictionary<string, string>();
        
        foreach (var filePath in Directory.EnumerateFiles(dirPath))
        {
            var fileName = Path.GetFileName(filePath);
            var fileBytes = File.ReadAllBytes(filePath);
            generatedClassFiles[fileName] = Convert.ToBase64String(fileBytes);
        }
        
        return Results.Ok(new CompileResultDto
        {
            GeneratedClassFiles = generatedClassFiles
        });
    }
    catch (DirectoryNotFoundException e)
    {
        var errorLog = File.ReadAllText($"/app/error-log/{compileId}/err.log");
        var cleanedErrorLog = errorLog.Replace($"/app/client-src/{compileId}/", "");
        return Results.BadRequest(new CompilationErrorDto(cleanedErrorLog));
    }
});

app.Run("http://0.0.0.0:5137");