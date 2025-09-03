using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.Configs;
using ExecutorService.Executor.Dtos;
using ExecutorService.Executor.Types;

namespace ExecutorService.Executor;

public interface ICompilationHandler
{
    public Task CompileAsync(UserSolutionData userSolutionData);
}

public class CompilerData
{
    internal int Pid { get; set; }
    internal int GuestCid { get; set; }
    internal Guid CompilerId { get; set; }
    internal DateTime CreatedAt { get; set; }
    internal string Status { get; set; } = "NOMINAL";
}

public sealed class CompilationHandler : ICompilationHandler
{
    private readonly ChannelWriter<CompileTask> _taskWriter;
    private readonly ChannelReader<CompileTask> _taskReader;

    private readonly ChannelWriter<CompilerData> _compilerDataWriter;
    private readonly ChannelReader<CompilerData> _compilerDataReader;
    
    public CompilationHandler()
    {
        var tasksToDispatch = Channel.CreateBounded<CompileTask>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
        });
        _taskWriter = tasksToDispatch.Writer;
        _taskReader = tasksToDispatch.Reader;
        
        var availableCompilerChannel = Channel.CreateUnbounded<CompilerData>();
        _compilerDataWriter = availableCompilerChannel.Writer;
        _compilerDataReader = availableCompilerChannel.Reader;
        
        for (var i = 0; i < 1; i++)
        {
            Task.Run(DispatchCompilationHandlers);
        }

        for (var i = 0; i < 1; i++)
        {
            var compilerData = DeployCompilerVmAsync(3 + i);

            _compilerDataWriter.TryWrite(compilerData);
        }
    }

    public async Task CompileAsync(UserSolutionData userSolutionData)
    {
        var compileTask = new TaskCompletionSource();
        await _taskWriter.WriteAsync(new CompileTask(userSolutionData, compileTask));
        await compileTask.Task;
    }

    private async Task DispatchCompilationHandlers()
    {
        while (true)
        {
            var task = await GetCompilationTask();
            var chosenCompiler = await GetAvailableCompilerId();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    ArgumentList =
                    {
                        "/app/firecracker/send-compilation.sh",
                        BuildWrappedRequestJson(task.UserSolutionData),
                        chosenCompiler.CompilerId.ToString()
                    },
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                }
            };
            process.Start();
            await process.WaitForExitAsync();

            
            task.Tcs.SetResult();
            
            _compilerDataWriter.TryWrite(chosenCompiler);
        }
    }

    private async Task<CompileTask> GetCompilationTask()
    {
        while (await _taskReader.WaitToReadAsync())
        {
            if (_taskReader.TryRead(out var task))
            {
                return task;
            }
        }

        throw new CompilationHandlerChannelReadException("Could not fetch task");
    }

    private async Task<CompilerData> GetAvailableCompilerId()
    {
        while (await _compilerDataReader.WaitToReadAsync())
        {
            if (_compilerDataReader.TryRead(out var task))
            {
                return task;
            } 
            await Task.Delay(10);
        }
        throw new CompilationHandlerChannelReadException("Could not fetch task");
    }
    
    private static CompilerData DeployCompilerVmAsync(int guestCid) 
    {
        var compilerData = new CompilerData
        {
            CompilerId =  Guid.NewGuid(),
            GuestCid = guestCid,
            CreatedAt = DateTime.Now,
        };
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"/app/firecracker/launch-compiler.sh \"{compilerData.CompilerId}\" \"{compilerData.GuestCid}\""
            }
        };
        process.Start();
        return compilerData;
    }

    private static string BuildWrappedRequestJson(UserSolutionData userSolutionData)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(userSolutionData.FileContents.ToString());
        var userCodeB64 = Convert.ToBase64String(plainTextBytes);
        return $"{{\"endpoint\":\"compile\",\"method\":\"POST\",\"content\":\"{{\\\"SrcCodeB64\\\":\\\"{userCodeB64}\\\",\\\"ClassName\\\":\\\"{userSolutionData.MainClassName}\\\",\\\"ExecutionId\\\":\\\"{userSolutionData.ExecutionId}\\\"}}\",\"ctype\":\"application/json\"}}";
        
    }
}
