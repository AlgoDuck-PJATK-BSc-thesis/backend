using System.Collections.Concurrent;
using System.Threading.Channels;
using ExecutorService.Executor.Helpers;
using ExecutorService.Executor.Types.Config;
using ExecutorService.Executor.Types.FilesystemPoolerTypes;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Extensions;

namespace ExecutorService.Executor.ResourceHandlers;

internal sealed class FilesystemRequest(FilesystemType fsType, TaskCompletionSource<RequestProcessingData> tcs)
{
    internal FilesystemType Filesystem { get; } = fsType;
    internal TaskCompletionSource<RequestProcessingData> Tcs { get; } = tcs;
}


public sealed class FilesystemPooler : IAsyncDisposable
{
    private readonly TimeSpan _trackingPeriod = TimeSpan.FromMinutes(10);
    private readonly TimeSpan _pollingFrequency = TimeSpan.FromSeconds(15);

    private const int DefaultCompilerCacheTarget = 1;
    private const int DefaultExecutorCacheTarget = 5;

    private double _safetyBuffer = 1.5;

    private readonly ChannelWriter<FilesystemRequest> _requestWriter;
    private readonly ChannelReader<FilesystemRequest> _requestReader;

    private readonly ConcurrentDictionary<FilesystemType, FilesystemChannel<Guid>> _channels;
    private readonly ConcurrentDictionary<FilesystemType, CacheTargetData> _cacheTargets;
    private readonly ConcurrentDictionary<FilesystemType, ConcurrentQueue<FilesystemRequestData>> _requestHistory;

    private readonly ILogger<FilesystemPooler> _logger;
    private readonly CancellationTokenSource _shutdownCts;
    private readonly Task _requestServicingDaemon;
    private readonly Task _cacheMaintenanceDaemon;
    
    private readonly IOptions<FileSystemPoolerConfig> _config;

    private bool _disposed;

    internal static async Task<FilesystemPooler> CreateFileSystemPoolerAsync(ILogger<FilesystemPooler> logger, IOptions<FileSystemPoolerConfig> config)
    {
        var fsPooler = new FilesystemPooler(logger, config);
        await fsPooler.MaintainCacheAsync();
        logger.LogInformation("FilesystemPooler initialized successfully");
        return fsPooler;
    }

    private FilesystemPooler(ILogger<FilesystemPooler> logger, IOptions<FileSystemPoolerConfig> config)
    {
        _logger = logger;
        _config = config;
        _shutdownCts = new CancellationTokenSource();

        var fsRequests = Channel.CreateUnbounded<FilesystemRequest>();
        _requestReader = fsRequests.Reader;
        _requestWriter = fsRequests.Writer;

        _channels = new ConcurrentDictionary<FilesystemType, FilesystemChannel<Guid>>
        {
            [FilesystemType.Executor] = new(Channel.CreateUnbounded<Guid>()),
            [FilesystemType.Compiler] = new(Channel.CreateUnbounded<Guid>()),
        };

        _cacheTargets = new ConcurrentDictionary<FilesystemType, CacheTargetData>
        {
            [FilesystemType.Compiler] = new(1, DefaultCompilerCacheTarget, int.MaxValue),
            [FilesystemType.Executor] = new(10, DefaultExecutorCacheTarget, int.MaxValue)
        };

        _requestHistory = new ConcurrentDictionary<FilesystemType, ConcurrentQueue<FilesystemRequestData>>
        {
            [FilesystemType.Compiler] = new(),
            [FilesystemType.Executor] = new(),
        };

        _requestServicingDaemon = Task.Run(() => ServiceFsRequestsAsync(_shutdownCts.Token));
        _cacheMaintenanceDaemon = Task.Run(() => MonitorCacheStateAsync(_shutdownCts.Token));

        _logger.LogDebug("FilesystemPooler daemon tasks started");
    }

    private async Task MaintainCacheAsync()
    {
        foreach (var fileSystemType in _requestHistory.Keys)
        {
            CalculateCacheTargets(fileSystemType);
            await VerifyAndRestoreCacheStateAsync(fileSystemType);
        }
    }

    private async Task MonitorCacheStateAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cache maintenance daemon started with polling frequency of {Frequency}", _pollingFrequency);

        try
        {
            using var periodicTimer = new PeriodicTimer(_pollingFrequency);

            while (await periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                try
                {
                    _logger.LogDebug("Running scheduled cache maintenance");
                    await MaintainCacheAsync();
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error during cache maintenance cycle");
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

        _logger.LogInformation("Cache maintenance daemon stopped");
    }

    private async Task VerifyAndRestoreCacheStateAsync(FilesystemType fsType)
    {
        var currentCacheCount = _channels[fsType].Reader.Count;
        var targetCount = _cacheTargets[fsType].CacheTargetCurrent;
        var deficit = targetCount - currentCacheCount;

        if (deficit <= 0)
            return;

        var spawnedCopyProcesses = new List<Task<Guid>>(deficit);
        for (var i = 0; i < deficit; ++i)
        {
            spawnedCopyProcesses.Add(CreateFilesystemAsync(fsType));
        }

        try
        {
            var createdFilesystemGuids = await Task.WhenAll(spawnedCopyProcesses);

            foreach (var createdFilesystem in createdFilesystemGuids)
            {
                await _channels[fsType].Writer.WriteAsync(createdFilesystem);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create some cached filesystems for {FilesystemType}", fsType);
            throw;
        }
    }

    private void CalculateCacheTargets(FilesystemType fsType)
    {
        var filesystemRequests = _requestHistory[fsType];
        var cutoff = DateTime.UtcNow - _trackingPeriod;

        var expiredCount = 0;
        while (!filesystemRequests.IsEmpty &&
               filesystemRequests.TryPeek(out var result) &&
               result.RequestDate < cutoff)
        {
            if (filesystemRequests.TryDequeue(out _))
                expiredCount++;
        }

        if (expiredCount > 0)
            _logger.LogDebug("Pruned {Count} expired request records for {FilesystemType}", expiredCount, fsType);
        
        if (filesystemRequests.IsEmpty)
            return;

        var filesystemRequestsCount = filesystemRequests.Count;
        var requestsPerMinute = filesystemRequestsCount / _trackingPeriod.TotalMinutes;
        var cachedRequestCount = filesystemRequests.Count(req => req.IsCached);
        var cacheHitRatio = (double)cachedRequestCount / filesystemRequestsCount;

        _safetyBuffer = CalculateSafetyBuffer(cacheHitRatio);

        var newTarget = Math.Max( _cacheTargets[fsType].CacheTargetMin, Math.Min( (int)Math.Ceiling(requestsPerMinute * _safetyBuffer), _cacheTargets[fsType].CacheTargetMax));

        _cacheTargets[fsType].CacheTargetCurrent = newTarget;
    }

    private static double CalculateSafetyBuffer(double cacheRatio)
    {
        return Math.Clamp(1.5 / Math.Max(0.8, cacheRatio * 1.1), 1.2, 2.0);
    }

    public async Task<Guid> EnqueueFilesystemRequestAsync(FilesystemType fsType, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var requestCreationTime = DateTime.UtcNow;
        var fsTask = new TaskCompletionSource<RequestProcessingData>(TaskCreationOptions.RunContinuationsAsynchronously);

        _logger.LogDebug("Enqueueing filesystem request for {FilesystemType}", fsType);

        await _requestWriter.WriteAsync(new FilesystemRequest(fsType, fsTask), cancellationToken);

        var result = await fsTask.Task.WaitAsync(cancellationToken);

        _requestHistory[fsType].Enqueue(new FilesystemRequestData(requestCreationTime, result.IsCached));

        var elapsed = DateTime.UtcNow - requestCreationTime;
        _logger.LogInformation(
            "Filesystem request fulfilled for {FilesystemType}: id={FilesystemId}, cached={IsCached}, elapsed={Elapsed}ms",
            fsType, result.FilesystemId, result.IsCached, elapsed.TotalMilliseconds);

        return result.FilesystemId;
    }

    private async Task ServiceFsRequestsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request servicing daemon started");

        try
        {
            await foreach (var request in _requestReader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    _logger.LogDebug("Processing filesystem request for {FilesystemType}", request.Filesystem);

                    var (resultId, isCached) = await ReadOrCreateFilesystemAsync(request.Filesystem, CreateFilesystemAsync);

                    request.Tcs.SetResult(new RequestProcessingData(resultId, isCached));

                    _logger.LogDebug(
                        "Filesystem request completed for {FilesystemType}: id={Id}, fromCache={IsCached}",
                        request.Filesystem, resultId, isCached);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Failed to process filesystem request for {FilesystemType}", request.Filesystem);
                    request.Tcs.SetException(ex);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Request servicing daemon received shutdown signal");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Request servicing daemon terminated unexpectedly");
            throw;
        }

        _logger.LogInformation("Request servicing daemon stopped");
    }

    private async Task<(Guid item, bool isCached)> ReadOrCreateFilesystemAsync(
        FilesystemType fsType,
        Func<FilesystemType, Task<Guid>>? create = null)
    {
        var reader = _channels[fsType].Reader;

        if (reader.TryRead(out var item))
        {
            _logger.LogDebug("Retrieved cached filesystem {Id} for {FilesystemType}", item, fsType);
            return (item, true);
        }

        _logger.LogDebug("Cache miss for {FilesystemType}, creating new filesystem on-demand", fsType);

        if (create == null)
        {
            var newItem = await reader.ReadAsync();
            return (newItem, true);
        }

        return (await create(fsType), false);
    }

    private async Task<Guid> CreateFilesystemAsync(FilesystemType fsType)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogDebug("Starting filesystem creation for {FilesystemType}", fsType);

        var filesystemTypeName = fsType.GetDisplayName().ToLowerInvariant();
        var fsCopyProcess = ExecutorScriptHelper.CreateBashExecutionProcess("/app/firecracker/make-copy-image.sh", filesystemTypeName);

        fsCopyProcess.Start();
        await fsCopyProcess.WaitForExitAsync();

        var output = await fsCopyProcess.StandardOutput.ReadToEndAsync();
        var filesystemId = Guid.Parse(output.Trim());

        var elapsed = DateTime.UtcNow - startTime;
        _logger.LogDebug("Filesystem creation completed for {FilesystemType}: id={Id}, elapsed={Elapsed}ms", fsType, filesystemId, elapsed.TotalMilliseconds);

        return filesystemId;
    }

    internal static bool RemoveFilesystemById(Guid fsId, ILogger? logger = null)
    {
        var fsPath = $"/var/algoduck/filesystems/{fsId}.ext4";

        if (!File.Exists(fsPath))
        {
            logger?.LogDebug("Filesystem {Id} already removed or does not exist", fsId);
            return true;
        }

        try
        {
            File.Delete(fsPath);
            var removed = !File.Exists(fsPath);

            if (removed)
                logger?.LogInformation("Successfully removed filesystem {Id}", fsId);
            else
                logger?.LogWarning("Failed to remove filesystem {Id}: file still exists after deletion", fsId);

            return removed;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error removing filesystem {Id}", fsId);
            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        _logger.LogInformation("FilesystemPooler disposal initiated");
        await _shutdownCts.CancelAsync();

        _requestWriter.Complete();

        var shutdownTimeout = TimeSpan.FromSeconds(30);
        try
        {
            var daemonShutdownTask = Task.WhenAll(_requestServicingDaemon, _cacheMaintenanceDaemon);
            var completedTask = await Task.WhenAny(daemonShutdownTask, Task.Delay(shutdownTimeout));

            if (completedTask != daemonShutdownTask)
            {
                _logger.LogWarning("Daemon tasks did not complete within {Timeout}s timeout during shutdown", shutdownTimeout.TotalSeconds);
            }
            else
            {
                await daemonShutdownTask;
                _logger.LogInformation("All daemon tasks completed gracefully");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Daemon tasks cancelled during shutdown");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during daemon task shutdown");
        }

        foreach (var (fsType, channel) in _channels)
        {
            channel.Writer.Complete();
            _logger.LogDebug("Completed channel for {FilesystemType}", fsType);
        }

        _shutdownCts.Dispose();

        _logger.LogInformation("FilesystemPooler disposal completed");
    }
}