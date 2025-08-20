using System.Diagnostics;
using ExecutorService.Executor.Dtos;
using ExecutorService.Executor.Types;

namespace ExecutorService.Executor;

public static class ExecutorScriptHandler
{
    public static Task CopyTemplateFs(UserSolutionData userSolutionData)
    {
        var buildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"/app/fc-scripts/copy-template-fs.sh \"{userSolutionData.MainClassName}\" \"{userSolutionData.ExecutionId}\" \"{userSolutionData.SigningKey}\"", 
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }
        };
        
        buildProcess.Start();
        return buildProcess.WaitForExitAsync();
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

        var buildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"/app/fc-scripts/populate-execution-fs.sh \"{userSolutionData.MainClassName}\" \"{userSolutionData.ExecutionId}\"", 
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }
        };
    
        buildProcess.Start();
        await buildProcess.WaitForExitAsync();
        Directory.Delete(bytecodeDirPath);
    }

    public static async Task ExecuteJava(UserSolutionData userSolutionData)
    {
        var execProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"/app/fc-scripts/java-exec.sh {userSolutionData.ExecutionId} {userSolutionData.SigningKey}",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }
        };

        execProcess.Start();
        await execProcess.WaitForExitAsync();
    }
}