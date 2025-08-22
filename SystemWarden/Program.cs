using System.Diagnostics;
using System.Globalization;
using System.Text;
using SystemWarden.Dtos;
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

app.MapPost("/execution-fs", async ([FromQuery] string executionId) =>
{
    var filesystemCreationProcess = CreateBashExecutionProcess("/app/Scripts/create-and-mount-fs.sh", executionId);
    filesystemCreationProcess.Start();
    await filesystemCreationProcess.WaitForExitAsync();
    Console.WriteLine("create");
    Console.WriteLine(await filesystemCreationProcess.StandardOutput.ReadToEndAsync());
    Console.WriteLine(await filesystemCreationProcess.StandardError.ReadToEndAsync());
    return Results.NoContent();
});

app.MapDelete("/umount", async ([FromQuery] string executionId) =>
{
    var filesystemUnmountingProcess = CreateBashExecutionProcess("/app/Scripts/umount.sh", executionId);
    filesystemUnmountingProcess.Start();
    await filesystemUnmountingProcess.WaitForExitAsync();
    Console.WriteLine("unmount");
    Console.WriteLine(await filesystemUnmountingProcess.StandardOutput.ReadToEndAsync());
    Console.WriteLine(await filesystemUnmountingProcess.StandardError.ReadToEndAsync());
    return Results.NoContent();
});

static async Task<string> DeployCompiler(int port, string mem, double cpus)
{
    var launchProcess = CreateBashExecutionProcess("/app/Scripts/deploy-compiler.sh", port.ToString(), mem, cpus.ToString(CultureInfo.InvariantCulture));
    launchProcess.Start();
    await launchProcess.WaitForExitAsync();
    var containerId = launchProcess.StandardOutput.ReadToEnd();
    return containerId;
}

static async Task KillCompiler(string containerId)
{
    var killProcess = CreateBashExecutionProcess("/app/Scripts/kill-compiler.sh", containerId);
    killProcess.Start();
    await killProcess.WaitForExitAsync();
}

static Process CreateBashExecutionProcess(string scriptPath, params string[] arguments)
{
    var scriptArguments = new StringBuilder();
    
    for (var i = 0; i < arguments.Length - 1; i++)
    {
        scriptArguments.Append($"{arguments[i]} ");
    }

    scriptArguments.Append(arguments[^1]);
    
    return new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"{scriptPath} {scriptArguments}", 
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }
    };
}

app.Run("http://0.0.0.0:7139");

