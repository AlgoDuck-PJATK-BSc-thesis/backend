using System.Threading.Channels;
using AlgoDuckShared;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.Types;
using ExecutorService.Executor.Types.Config;
using ExecutorService.Executor.Types.FilesystemPoolerTypes;
using ExecutorService.Executor.Types.VmLaunchTypes;
using ExecutorService.Executor.VmLaunchSystem;
using Microsoft.Extensions.Options;

namespace ExecutorService.Executor.ResourceHandlers;

public interface ICompilationHandler
{
    internal Task<VmCompilationResponse> CompileAsync(VmJobRequestInterface<VmCompilationPayload> request);
}

internal sealed class CompilationHandler : ICompilationHandler
{
    private readonly ChannelWriter<CompileTask> _taskWriter;
    private readonly ChannelReader<CompileTask> _taskReader;

    private readonly ChannelWriter<VmLease> _compilerDataWriter;
    private readonly ChannelReader<VmLease> _compilerDataReader;
    private readonly IOptions<CompilationHandlerConfig> _options;
    private readonly ILogger<CompilationHandler> _logger;


    private CompilationHandler(IOptions<CompilationHandlerConfig> options, ILogger<CompilationHandler> logger)
    {
        _options = options;
        _logger = logger;
        var tasksToDispatch = Channel.CreateBounded<CompileTask>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait,
        });
        _taskWriter = tasksToDispatch.Writer;
        _taskReader = tasksToDispatch.Reader;

        var availableCompilerChannel = Channel.CreateUnbounded<VmLease>();
        _compilerDataWriter = availableCompilerChannel.Writer;
        _compilerDataReader = availableCompilerChannel.Reader;
        
        var shutdownCts = new CancellationTokenSource();

        for (var i = 0; i < _options.Value.WorkerThreadCount; i++)
        {
            Task.Run(() => DispatchCompilationHandler(shutdownCts.Token));
        }
    }

    public async Task<VmCompilationResponse> CompileAsync(VmJobRequestInterface<VmCompilationPayload> request)
    {
        var compileTask = new TaskCompletionSource<VmCompilationResponse>();
        await _taskWriter.WriteAsync(new CompileTask(request, compileTask));
        return await compileTask.Task;
    }

    private async Task DispatchCompilationHandler(CancellationToken cancellationToken = default)
    {
        try
        {
            using var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(_options.Value.PollingFrequencyMs));

            while (await periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                var task = await GetCompilationTask();
                var compilerLease = await GetAvailableCompilerId();
                try
                {
                    var result =
                        await compilerLease.QueryAsync<VmCompilationPayload, VmCompilationResponse>(task.Request);
                    task.Tcs.SetResult(result);
                }
                catch (VmQueryTimedOutException ex)
                {
                    compilerLease = await ex.WatchDogDecision!;
                }
                finally
                {
                    await ReturnCompilerToPool(compilerLease);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Cache maintenance daemon received shutdown signal");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Cache maintenance daemon terminated unexpectedly");
            throw;
        }
    }
    public static async Task<CompilationHandler> CreateAsync(VmLaunchManager launchManager, IOptions<CompilationHandlerConfig> options, ILogger<CompilationHandler> logger)
    {
        var handler = new CompilationHandler(options, logger);
        
        for (var i = 0; i < options.Value.DefaultCompilerCount; i++)
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