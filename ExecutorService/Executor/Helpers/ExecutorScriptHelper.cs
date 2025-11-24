using System.Diagnostics;
using System.Text;

namespace ExecutorService.Executor.Helpers;

public class VmOutput
{
    public string? Out { get; init; }
    public string? Err { get; init; }
}

internal static class ExecutorScriptHelper
{
    internal static Process CreateBashExecutionProcess(string scriptPath, params string[] arguments)
    {
        var scriptArguments = new StringBuilder();
    
        for (var i = 0; i < arguments.Length - 1; i++)
        {
            scriptArguments.Append($"\"{arguments[i]}\" ");
        }

        if (arguments.Length > 0)
        {
            scriptArguments.Append($"\"{arguments[^1]}\"");
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