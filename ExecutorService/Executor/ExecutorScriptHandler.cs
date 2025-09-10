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

public static class ExecutorScriptHandler
{
    internal static void LaunchExecutor(UserSolutionData userSolutionData)
    {
        var launchProcess = CreateBashExecutionProcess("/app/firecracker/launch-executor.sh", userSolutionData.ExecutionId.ToString(), "10");
        launchProcess.Start();
    }

    internal static async Task SendExecutionData(UserSolutionData userSolutionData)
    {
        var sendProcess = CreateBashExecutionProcess("/app/firecracker/send-execution.sh", userSolutionData.ExecutionId.ToString());
        sendProcess.Start();
        await sendProcess.WaitForExitAsync();
    }
    
    private static Process CreateBashExecutionProcess(string scriptPath, params string[] arguments)
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
}