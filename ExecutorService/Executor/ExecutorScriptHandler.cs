using System.Diagnostics;
using System.Text;
using ExecutorService.Executor.Dtos;
using ExecutorService.Executor.Types;

namespace ExecutorService.Executor;

public static class ExecutorScriptHandler
{
    public static async Task CopyTemplateFs(UserSolutionData userSolutionData)
    {
        var buildProcess = CreateBashExecutionProcess( "/app/fc-scripts/copy-template-fs.sh",
            userSolutionData.MainClassName, userSolutionData.ExecutionId.ToString(), userSolutionData.SigningKey.ToString());
        
        buildProcess.Start();
        await buildProcess.WaitForExitAsync();
    }    
    
    public static async Task PopulateCopyFs(UserSolutionData userSolutionData, CompileResultDto userByteCode)
    {
        var bytecodeDirPath = $"/tmp/{userSolutionData.ExecutionId}/bytecode";
        Directory.CreateDirectory(bytecodeDirPath);
        foreach (var generatedClassFile in userByteCode.GeneratedClassFiles)
        {
            var fileBytes = Convert.FromBase64String(generatedClassFile.Value);
            await File.WriteAllBytesAsync($"{bytecodeDirPath}/{generatedClassFile.Key}", fileBytes);
        }

        var buildProcess = CreateBashExecutionProcess("/app/fc-scripts/populate-execution-fs.sh",
            userSolutionData.MainClassName, userSolutionData.ExecutionId.ToString());
        
        buildProcess.Start();
        await buildProcess.WaitForExitAsync();
        Directory.Delete(bytecodeDirPath);
    }

    public static async Task ExecuteJava(UserSolutionData userSolutionData)
    {
        var execProcess = CreateBashExecutionProcess("/app/fc-scripts/java-exec.sh",
            userSolutionData.ExecutionId.ToString(), userSolutionData.SigningKey.ToString());
        execProcess.Start();
        await execProcess.WaitForExitAsync();
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