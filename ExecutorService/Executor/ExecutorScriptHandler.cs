using System.Diagnostics;
using System.Text;
using ExecutorService.Executor.Dtos;
using ExecutorService.Executor.Types;

namespace ExecutorService.Executor;

public class VmOutput
{
    public string? Out { get; init; }
    public string? Err { get; init; }
}

internal static class ExecutorScriptHandler
{
    // private readonly FilesystemPooler _pooler;

    // internal ExecutorScriptHandler(FilesystemPooler pooler)
    // {
    //     _pooler = pooler;
    // }

    internal static async Task LaunchExecutor(UserSolutionData userSolutionData)
    {;
        // var executorFilesystemId = await _pooler.EnqueueFilesystemRequestAsync(FilesystemType.Executor); 
        var launchProcess = CreateBashExecutionProcess("/app/firecracker/launch-executor.sh", userSolutionData.ExecutionId.ToString(), "10"/*, executorFilesystemId.ToString()*/);
        launchProcess.Start();
        await launchProcess.WaitForExitAsync();
        var readToEndAsync = await launchProcess.StandardOutput.ReadToEndAsync();
        Console.WriteLine($"Executor Launch: {readToEndAsync}");
    }

    internal static async Task SendExecutionData(UserSolutionData userSolutionData)
    {
        var sendProcess = CreateBashExecutionProcess("/app/firecracker/send-execution.sh", userSolutionData.ExecutionId.ToString());
        sendProcess.Start();
        await sendProcess.WaitForExitAsync();
        var readToEndAsync = await sendProcess.StandardOutput.ReadToEndAsync();
        Console.WriteLine($"Execution: {readToEndAsync}");

        const string huh =
            "{\"entrypoint\":\"Main\",\"generatedClassFiles\":{\"Main.class\":\"yv66vgAAAD0ANAoAAgADBwAEDAAFAAYBABBqYXZhL2xhbmcvT2JqZWN0AQAGPGluaXQ+AQADKClWCgAIAAkHAAoMAAsADAEAEGphdmEvbGFuZy9TeXN0ZW0BABFjdXJyZW50VGltZU1pbGxpcwEAAygpSgkACAAODAAPABABAANvdXQBABVMamF2YS9pby9QcmludFN0cmVhbTsIABIBAAxIZWxsbyB0ZXN0IDIKABQAFQcAFgwAFwAYAQATamF2YS9pby9QcmludFN0cmVhbQEAB3ByaW50bG4BABUoTGphdmEvbGFuZy9TdHJpbmc7KVYSAAAAGgwAGwAcAQAXbWFrZUNvbmNhdFdpdGhDb25zdGFudHMBABUoSilMamF2YS9sYW5nL1N0cmluZzsHAB4BAARNYWluAQAEQ29kZQEAD0xpbmVOdW1iZXJUYWJsZQEABG1haW4BABYoW0xqYXZhL2xhbmcvU3RyaW5nOylWAQAKU291cmNlRmlsZQEACU1haW4uamF2YQEAEEJvb3RzdHJhcE1ldGhvZHMPBgAnCgAoACkHACoMABsAKwEAJGphdmEvbGFuZy9pbnZva2UvU3RyaW5nQ29uY2F0RmFjdG9yeQEAmChMamF2YS9sYW5nL2ludm9rZS9NZXRob2RIYW5kbGVzJExvb2t1cDtMamF2YS9sYW5nL1N0cmluZztMamF2YS9sYW5nL2ludm9rZS9NZXRob2RUeXBlO0xqYXZhL2xhbmcvU3RyaW5nO1tMamF2YS9sYW5nL09iamVjdDspTGphdmEvbGFuZy9pbnZva2UvQ2FsbFNpdGU7CAAtAQAwY3RyLTk0MWRkNmVjLTY5MzItNGY1OC1iN2JmLTgzNzg3N2VlMjA0NS10aW1lOiABAQAMSW5uZXJDbGFzc2VzBwAwAQAlamF2YS9sYW5nL2ludm9rZS9NZXRob2RIYW5kbGVzJExvb2t1cAcAMgEAHmphdmEvbGFuZy9pbnZva2UvTWV0aG9kSGFuZGxlcwEABkxvb2t1cAAhAB0AAgAAAAAAAgABAAUABgABAB8AAAAdAAEAAQAAAAUqtwABsQAAAAEAIAAAAAYAAQAAAAEACQAhACIAAQAfAAAARwAFAAUAAAAfuAAHQLIADRIRtgATuAAHQrIADSEfZboAGQAAtgATsQAAAAEAIAAAABYABQAAAAIABAAEAAwABQAQAAYAHgAHAAMAIwAAAAIAJAAlAAAACAABACYAAQAsAC4AAAAKAAEALwAxADMAGQ==\"}}";
    }
    
    internal static Process CreateBashExecutionProcess(string scriptPath, params string[] arguments)
    {
        var scriptArguments = new StringBuilder();
    
        for (var i = 0; i < arguments.Length - 1; i++)
        {
            scriptArguments.Append($"{arguments[i]} ");
        }

        if (arguments.Length > 0)
        {
            scriptArguments.Append(arguments[^1]);
        }

        var args = $"{scriptPath} {scriptArguments}";
        return new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = args, 
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
    }
}