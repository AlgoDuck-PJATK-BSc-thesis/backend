using System.Collections.Concurrent;
using System.Threading.Channels;
using ExecutorService.Executor.Helpers;
using Microsoft.OpenApi.Extensions;

namespace ExecutorService.Executor.ResourceHandlers;

public enum FilesystemType
{
    Compiler, Executor
}

internal class FilesystemRequestData(DateTime requestDate, bool isCached)
{
    internal DateTime RequestDate { get; init; } = requestDate;
    internal bool IsCached { get; init; } = isCached;
}

internal class RequestProcessingData(Guid filesystemId, bool isCached)
{
    internal Guid FilesystemId { get; init; } = filesystemId;
    internal bool IsCached { get; init; } = isCached;
}

internal class CacheTargetData(int cacheTargetMin, int cacheTargetInit, int cacheTargetMax)
{
    internal int CacheTargetMin { get; } = cacheTargetMin;
    internal int CacheTargetCurrent { get; set; } = cacheTargetInit;
    internal int CacheTargetMax { get; } = cacheTargetMax;
}

internal class FilesystemChannel<T>(Channel<T> channel)
{
    internal ChannelReader<T> Reader { get; } = channel.Reader;
    internal ChannelWriter<T> Writer { get; } = channel.Writer;
    
}

internal class FilesystemRequest(FilesystemType fsType, TaskCompletionSource<RequestProcessingData> tcs)
{
    internal FilesystemType Filesystem => fsType;
    internal TaskCompletionSource<RequestProcessingData> Tcs => tcs;
}

internal interface IFilesystemPooler
{
    internal Task<Guid> EnqueueFilesystemRequestAsync(FilesystemType fsType);
}

public sealed class FilesystemPooler : IFilesystemPooler
{
    private readonly TimeSpan _trackingPeriod = TimeSpan.FromMinutes(10);
    private readonly TimeSpan _pollingFrequency = TimeSpan.FromSeconds(15);

    private const int DEFAULT_COMPILER_CACHE_TARGET = 1;
    private const int DEFAULT_EXECUTOR_CACHE_TARGET = 5;
    
    private double _safetyBuffer = 1.5;
    
    private readonly ChannelWriter<FilesystemRequest> _requestWriter;
    private readonly ChannelReader<FilesystemRequest> _requestReader;
    
    private readonly ConcurrentDictionary<FilesystemType, FilesystemChannel<Guid>> _channels;
    private readonly ConcurrentDictionary<FilesystemType, CacheTargetData> _cacheTargets;
    private readonly ConcurrentDictionary<FilesystemType, ConcurrentQueue<FilesystemRequestData>> _requestHistory;


    internal static async Task<FilesystemPooler> CreateFileSystemPoolerAsync()
    {
        var fsPooler = new FilesystemPooler();
        await fsPooler.MaintainCacheAsync();
        return fsPooler;
    }
    
    private FilesystemPooler()
    {
        var fsRequests = Channel.CreateUnbounded<FilesystemRequest>();
        _requestReader = fsRequests.Reader;
        _requestWriter = fsRequests.Writer;


        var executorFilesystems = Channel.CreateUnbounded<Guid>();

        var compilerFilesystems = Channel.CreateUnbounded<Guid>();
        _channels = new ConcurrentDictionary<FilesystemType, FilesystemChannel<Guid>>
        {
            [FilesystemType.Executor] = new(executorFilesystems),
            [FilesystemType.Compiler] = new(compilerFilesystems),
            
        };
        
        _cacheTargets = new ConcurrentDictionary<FilesystemType, CacheTargetData>
        {
            [FilesystemType.Compiler] = new(1, DEFAULT_COMPILER_CACHE_TARGET, int.MaxValue),
            [FilesystemType.Executor] = new(10, DEFAULT_EXECUTOR_CACHE_TARGET, int.MaxValue)
        };

        _requestHistory = new ConcurrentDictionary<FilesystemType, ConcurrentQueue<FilesystemRequestData>>
        {
            [FilesystemType.Compiler] = new(),
            [FilesystemType.Executor] = new(),
        };
        Task.Run(ServiceFsRequests);
        
        Task.Run(MonitorCacheState);
        
    }

    private async Task MaintainCacheAsync()
    {
        foreach (var fileSystemType in _requestHistory.Keys)
        {
            
            CalculateCacheTargets(fileSystemType);
            
            await VerifyAndRestoreCacheStateAsync(fileSystemType);
        }
    }

    private async Task MonitorCacheState()
    {
        var periodicTimer = new PeriodicTimer(_pollingFrequency);
        while (await periodicTimer.WaitForNextTickAsync())
        {
            await MaintainCacheAsync();
        }
    }

    private async Task VerifyAndRestoreCacheStateAsync(FilesystemType fsType)
    {
        var diff = _cacheTargets[fsType].CacheTargetCurrent - _channels[fsType].Reader.Count;
        List<Task<Guid>> spawnedCopyProcesses = [];
        for (var i = 0; i < diff; ++i)
        {
            spawnedCopyProcesses.Add(CreateFilesystem(fsType));
        }

        var createdFilesystemGuids = await Task.WhenAll(spawnedCopyProcesses);
        foreach (var createdFilesystem in createdFilesystemGuids)
        {
            await _channels[fsType].Writer.WriteAsync(createdFilesystem);
        }
    }

    private void CalculateCacheTargets(FilesystemType fsType)
    {
        var filesystemRequests = _requestHistory[fsType];
        var cutoff = DateTime.UtcNow - _trackingPeriod;

        while (!filesystemRequests.IsEmpty && filesystemRequests.TryPeek(out var result) && result.RequestDate < cutoff)
        {
            filesystemRequests.TryDequeue(out _);
        }
        
        if (filesystemRequests.IsEmpty) return;
        
        var filesystemRequestsCount = filesystemRequests.Count;
        var requestsPerMinute = filesystemRequestsCount / _trackingPeriod.TotalMinutes;
        var cachedRequestCount = filesystemRequests.Count(req => req.IsCached);
        var cacheRatio = cachedRequestCount / filesystemRequestsCount;
        
        _safetyBuffer = CalculateSafetyBuffer(cacheRatio);  // slightly lower safety buffer if cache ratio is super high, slightly raise it if it's okay and raise it by no more than twofold if cache ratio is low 
        var newTarget = (int) Math.Ceiling(requestsPerMinute * _safetyBuffer);
        
        _cacheTargets[fsType].CacheTargetCurrent = newTarget;
    }
    
    private static double CalculateSafetyBuffer(double cacheRatio)
    {
        return Math.Clamp(1.5 / Math.Max(0.8, cacheRatio * 1.1), 1.2, 2.0);
    }
    

    public async Task<Guid> EnqueueFilesystemRequestAsync(FilesystemType fsType)
    {
        Console.WriteLine($"Placing fs request: {DateTime.UtcNow}");
        var requestCreationTime = DateTime.UtcNow;
        var fsTask = new TaskCompletionSource<RequestProcessingData>(TaskCreationOptions.RunContinuationsAsynchronously);
        Console.WriteLine($"Writing fs request: {DateTime.UtcNow}");
        await _requestWriter.WriteAsync(new FilesystemRequest(fsType, fsTask));
        Console.WriteLine($"Writiten fs request: {DateTime.UtcNow}");
        var result = await fsTask.Task;
        Console.WriteLine($"Awaited fs request: {DateTime.UtcNow}");
        _requestHistory[fsType].Enqueue(new FilesystemRequestData(requestCreationTime, result.IsCached));
        Console.WriteLine($"Got fs: {DateTime.UtcNow}");
        return result.FilesystemId;
    }


    private async Task ServiceFsRequests()
    {
        while (true)
        {
            Console.WriteLine("reading requests");
            var request = await _requestReader.ReadAsync();
            Console.WriteLine("read request");
            var (resultId, isCached) = await ReadOrCreateFilesystemAsync(request.Filesystem, CreateFilesystem);
            request.Tcs.SetResult(new RequestProcessingData(resultId, isCached));
        }
    }

    
    private async Task<(Guid item, bool isCached)> ReadOrCreateFilesystemAsync(FilesystemType fsType, Func<FilesystemType, Task<Guid>>? create = null)
    {
        var reader = _channels[fsType].Reader;
        Console.WriteLine("reading fs");
        if (reader.TryRead(out var item))
            return (item, true);
        Console.WriteLine("creating fs");
        return create == null ? (await reader.ReadAsync(), true) : (await create(fsType), false);
    }

    private static async Task<Guid> CreateFilesystem(FilesystemType fsType)
    {
        Console.WriteLine($"Creating new fs {DateTime.UtcNow}");
        var filesystemTypeName = fsType.GetDisplayName().ToLowerInvariant();
        var fsCopyProcess = ExecutorScriptHelper.CreateBashExecutionProcess("/app/firecracker/make-copy-image.sh", filesystemTypeName);

        fsCopyProcess.Start();
        await fsCopyProcess.WaitForExitAsync();
        Console.WriteLine($"Creating new fs end {DateTime.UtcNow}");

        var output = await fsCopyProcess.StandardOutput.ReadToEndAsync();
        return Guid.Parse(output.Trim());
    }

    internal static bool RemoveFilesystemById(Guid fsId)
    {
        var fsPath = $"/var/algoduck/filesystems/{fsId}.ext4";
        if (!File.Exists(fsPath)) return true;
        File.Delete(fsPath);
        return !File.Exists(fsPath);
    }
    
}