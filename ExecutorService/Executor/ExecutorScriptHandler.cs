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

    internal static void LaunchExecutor(UserSolutionData userSolutionData)
    {;
        // var executorFilesystemId = await _pooler.EnqueueFilesystemRequestAsync(FilesystemType.Executor); 
        var launchProcess = CreateBashExecutionProcess("/app/firecracker/launch-executor.sh", userSolutionData.ExecutionId.ToString(), "10"/*, executorFilesystemId.ToString()*/);
        launchProcess.Start();
    }

    internal static async Task SendExecutionData(UserSolutionData userSolutionData)
    {
        var sendProcess = CreateBashExecutionProcess("/app/firecracker/send-execution.sh", userSolutionData.ExecutionId.ToString());
        sendProcess.Start();
        await sendProcess.WaitForExitAsync();
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