using System.Diagnostics;
using DockerWarden.Dtos;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

var portMappedToContainerIds = new Dictionary<int, string>();

app.MapPost("/container", async ([FromBody] ContainerCreationRequestDto containerCreationRequestDto) =>
{
    if (portMappedToContainerIds.ContainsKey(containerCreationRequestDto.Port))
    {
        return Results.BadRequest($"Container mapped to port: {containerCreationRequestDto.Port} already exists");
    }

    var containerId = await DeployCompiler(containerCreationRequestDto.Port, containerCreationRequestDto.Mem, containerCreationRequestDto.Cpus);
    portMappedToContainerIds[containerCreationRequestDto.Port] = containerId;

    return Results.Ok(new ContainerCreationResponseDto(containerId));
});

app.MapDelete("/container/{port:int}", async (int port) =>
{
    await KillCompiler(portMappedToContainerIds[port]);
    portMappedToContainerIds.Remove(port);
    return Results.NoContent();
});

static async Task<string> DeployCompiler(int port, string mem, double cpus)
{
    var launchProcess = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"/app/Scripts/deploy-compiler.sh {port} {mem} {cpus}", 
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }
    };
    launchProcess.Start();
    await launchProcess.WaitForExitAsync();
    var containerId = launchProcess.StandardOutput.ReadToEnd();
    return containerId;
}

static async Task KillCompiler(string containerId)
{
    var killProcess = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"/app/Scripts/kill-compiler.sh {containerId}", 
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }
    };
    
    killProcess.Start();
    await killProcess.WaitForExitAsync();
}

app.Run("http://0.0.0.0:7139");

