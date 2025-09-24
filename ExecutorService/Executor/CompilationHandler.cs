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
    internal Task<VmCompilationResponse> CompileAsync(UserSolutionData userSolutionData);
}

public class CompilerData
{
    internal Guid CompilerId { get; set; }
}

internal sealed class CompilationHandler : ICompilationHandler
{
    private readonly ChannelWriter<CompileTask> _taskWriter;
    private readonly ChannelReader<CompileTask> _taskReader;

    private readonly ChannelWriter<CompilerData> _compilerDataWriter;
    private readonly ChannelReader<CompilerData> _compilerDataReader;

    private readonly VmLaunchManager _launchManager;
    
    private CompilationHandler(VmLaunchManager launchManager)
    {
        _launchManager = launchManager;
        
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
    }

    public async Task<VmCompilationResponse> CompileAsync(UserSolutionData userSolutionData)
    {
        var compileTask = new TaskCompletionSource<VmCompilationResponse>();
        await _taskWriter.WriteAsync(new CompileTask(userSolutionData, compileTask));
        return await compileTask.Task;
    }

    private async Task DispatchCompilationHandlers()
    {
        while (true)
        {
            var task = await GetCompilationTask();
            var chosenCompiler = await GetAvailableCompilerId();
            
            var result = await _launchManager.QueryVm<VmCompilationQuery, VmCompilationResponse>(chosenCompiler.CompilerId, new VmCompilationQuery()
            {
                Endpoint = "compile",
                Method = HttpMethod.Post,
                Content = new VmCompilationQueryContent
                {
                    ClassName = task.UserSolutionData.MainClassName,
                    ExecutionId = task.UserSolutionData.ExecutionId,
                    SrcCodeB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(task.UserSolutionData.FileContents.ToString()))
                }
            });
            await ReturnCompilerToPool(chosenCompiler);
            task.Tcs.SetResult(result);
        }
    }
    public static async Task<CompilationHandler> CreateAsync(FilesystemPooler pooler, VmLaunchManager launchManager)
    {
        var handler = new CompilationHandler(launchManager);
        
        for (var i = 0; i < 1; i++)
        {
            var compilerGuid = await launchManager.DispatchVm(FilesystemType.Compiler);
            
            handler._compilerDataWriter.TryWrite(new CompilerData()
            {
                CompilerId = compilerGuid,
            });
        }
        
        return handler;
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

    private async Task ReturnCompilerToPool(CompilerData data)
    {
        while (await _compilerDataWriter.WaitToWriteAsync())
        {
            if (_compilerDataWriter.TryWrite(data))
            {
                return;
            }

            await Task.Delay(10);
        }
        throw new CompilationHandlerChannelReadException("Could not fetch task");
    }
}
