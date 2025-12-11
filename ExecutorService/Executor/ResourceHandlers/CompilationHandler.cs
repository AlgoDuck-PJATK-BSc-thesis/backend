using System.Threading.Channels;
using AlgoDuckShared;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.Types;
using ExecutorService.Executor.Types.VmLaunchTypes;
using ExecutorService.Executor.VmLaunchSystem;

namespace ExecutorService.Executor.ResourceHandlers;

public interface ICompilationHandler
{
    internal Task<VmCompilationResponse> CompileAsync(SubmitExecuteRequestRabbit request);
}

internal sealed class CompilationHandler : ICompilationHandler
{
    private readonly ChannelWriter<CompileTask> _taskWriter;
    private readonly ChannelReader<CompileTask> _taskReader;

    private readonly ChannelWriter<VmLease> _compilerDataWriter;
    private readonly ChannelReader<VmLease> _compilerDataReader;

    private CompilationHandler()
    {
        var tasksToDispatch = Channel.CreateBounded<CompileTask>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
        });
        _taskWriter = tasksToDispatch.Writer;
        _taskReader = tasksToDispatch.Reader;
        
        var availableCompilerChannel = Channel.CreateUnbounded<VmLease>();
        _compilerDataWriter = availableCompilerChannel.Writer;
        _compilerDataReader = availableCompilerChannel.Reader;
        
        for (var i = 0; i < 1; i++)
        {
            Task.Run(DispatchCompilationHandlers);
        }
    }

    public async Task<VmCompilationResponse> CompileAsync(SubmitExecuteRequestRabbit request)
    {
        var compileTask = new TaskCompletionSource<VmCompilationResponse>();
        await _taskWriter.WriteAsync(new CompileTask(request, compileTask));
        return await compileTask.Task;
    }

    private async Task DispatchCompilationHandlers()
    {
        while (true)
        {
            var task = await GetCompilationTask();
            var compilerLease = await GetAvailableCompilerId();
            try
            {
                var result = await compilerLease.QueryAsync<VmCompilationQuery<VmCompilationQueryContent>, VmCompilationResponse>(
                    new VmCompilationQuery<VmCompilationQueryContent>
                    {
                        Endpoint = "compile",
                        Method = HttpMethod.Post,
                        Content = new VmCompilationQueryContent
                        {
                            ClassName = task.Request.JavaFiles.First().Key,
                            ExecutionId = Guid.NewGuid(),
                            SrcCodeB64 = task.Request.JavaFiles.First().Value
                        }
                    });
                task.Tcs.SetResult(result);
            }
            catch (VmQueryTimedOutException ex)
            {
                compilerLease = await ex.WatchDogDecision;
            }
            finally
            {
                await ReturnCompilerToPool(compilerLease);
            }
        }
    }
    public static async Task<CompilationHandler> CreateAsync(VmLaunchManager launchManager)
    {
        var handler = new CompilationHandler();
        
        for (var i = 0; i < 1; i++)
        {
            var lease = await launchManager.AcquireVmAsync(FilesystemType.Compiler);
            handler._compilerDataWriter.TryWrite(lease);
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

    private async Task<VmLease> GetAvailableCompilerId()
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

    private async Task ReturnCompilerToPool(VmLease data)
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
