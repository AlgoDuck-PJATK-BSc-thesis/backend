using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.Helpers;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.Types.VmLaunchTypes;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Extensions;
using Polly;
using Polly.Timeout;

namespace ExecutorService.Executor.VmLaunchSystem;

public sealed class VmLaunchManager : IAsyncDisposable
{
    private readonly FilesystemPooler _pooler;
    private readonly ILogger<VmLaunchManager> _logger;
    private readonly ExecutorConfiguration _config;
    
    private readonly JsonSerializerOptions _defaultSerializerOptions = new() 
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly ConcurrentDictionary<FilesystemType, VmResourceAllocation> _defaultResourceAllocations;
    private readonly ConcurrentDictionary<Guid, VmConfig> _activeVms;
    private readonly VmWatchdog _watchdog;
    private readonly VmOversubManager _oversubManager;

    private int _nextGuestCid = 3;
    private int GetNextGuestCid() => Interlocked.Increment(ref _nextGuestCid);

    private readonly ConcurrentDictionary<FilesystemType, Channel<VmConfig>> _orphanPool;
    private readonly CancellationTokenSource _shutdownCts = new();

    public VmLaunchManager(
        FilesystemPooler pooler,
        ILogger<VmLaunchManager> logger,
        IOptions<ExecutorConfiguration> config)
    {
        _pooler = pooler;
        _logger = logger;
        _config = config.Value;
        _activeVms = new ConcurrentDictionary<Guid, VmConfig>();
        
        _defaultResourceAllocations = new ConcurrentDictionary<FilesystemType, VmResourceAllocation>
        {
            [FilesystemType.Executor] = new VmResourceAllocation
            {
                VcpuCount = _config.Resources.ExecutorVcpuCount,
                MemMB = _config.Resources.ExecutorMemoryMb
            },
            [FilesystemType.Compiler] = new VmResourceAllocation
            {
                VcpuCount = _config.Resources.CompilerVcpuCount,
                MemMB = _config.Resources.CompilerMemoryMb,
            }
        };

        _orphanPool = new ConcurrentDictionary<FilesystemType, Channel<VmConfig>>
        {
            [FilesystemType.Executor] = Channel.CreateBounded<VmConfig>(
                new BoundedChannelOptions(_config.Pool.OrphanPoolSize)
                {
                    FullMode = BoundedChannelFullMode.Wait, 
                })
        };
        
        _watchdog = new VmWatchdog(_activeVms);
        _oversubManager = new VmOversubManager(_activeVms, _defaultResourceAllocations);
    }

    private async Task<Guid> DispatchVmAsync(FilesystemType filesystemType, string? vmName = null, CancellationToken ct = default)
    {
        using var activity = new Activity("DispatchVm").Start();
        activity?.SetTag("filesystem_type", filesystemType.ToString());
        
        _logger.LogDebug("Dispatching {FilesystemType} VM", filesystemType);

        if (_orphanPool.TryGetValue(filesystemType, out var orphanChannel) && 
            orphanChannel.Reader.TryRead(out var orphanConfig))
        {
            _logger.LogDebug("Reusing orphan VM {VmId}", orphanConfig.VmId);
            return orphanConfig.VmId;                
        }
        
        var vmId = Guid.NewGuid();
        var guestCid = GetNextGuestCid();

        var createdVmConfig = new VmConfig
        {
            VmId = vmId,
            VmName = vmName ?? GenerateName(),
            AllocatedResources = _defaultResourceAllocations[filesystemType],
            FilesystemId = await _pooler.EnqueueFilesystemRequestAsync(filesystemType),
            GuestCid = guestCid,
            VmType = filesystemType,
            VsockPath = $"/var/algoduck/vsocks/{vmId}.vsock",
        };

        const string launchScriptPath = "/app/firecracker/launch-vm.sh";
        
        var launchProcess = ExecutorScriptHelper.CreateBashExecutionProcess(
            launchScriptPath,
            createdVmConfig.VmId.ToString(),
            createdVmConfig.GuestCid.ToString(), 
            createdVmConfig.FilesystemId.ToString(),
            createdVmConfig.AllocatedResources.VcpuCount.ToString(),
            createdVmConfig.AllocatedResources.MemMB.ToString(),
            createdVmConfig.AllocatedResources.Smt.ToString().ToLowerInvariant()
        );

        if (!await _oversubManager.EnqueueResourceRequest(ResourceRequestType.Spawn, filesystemType, ct))
        {
            _logger.LogWarning("Resource exhaustion prevented VM spawn for {FilesystemType}", filesystemType);
            throw new VmClusterOverloadedException($"Insufficient resources to spawn {filesystemType} VM");
        }

        var sw = Stopwatch.StartNew();
        launchProcess.Start();
        
        using var processCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        processCts.CancelAfter(_config.Timeouts.VmLaunchTimeout);
        
        try
        {
            await launchProcess.WaitForExitAsync(processCts.Token);
        }
        catch (OperationCanceledException)
        {
            launchProcess.Kill(entireProcessTree: true);
            throw new TimeoutException($"VM launch timed out after {_config.Timeouts.VmLaunchTimeout}");
        }
        
        sw.Stop();
        _logger.LogInformation("VM {VmId} launched in {ElapsedMs}ms", vmId, sw.ElapsedMilliseconds);

        var output = await launchProcess.StandardOutput.ReadToEndAsync(ct);
        var stderr = await launchProcess.StandardError.ReadToEndAsync(ct);
        
        if (launchProcess.ExitCode != 0)
        {
            _logger.LogError("VM launch failed: {StdErr}", stderr);
            throw new InvalidOperationException($"VM launch script failed with exit code {launchProcess.ExitCode}: {stderr}");
        }

        if (!int.TryParse(output.Trim(), out var pid))
        {
            _logger.LogError("Failed to parse PID from launch script output: {Output}", output);
            throw new InvalidOperationException($"Invalid PID from launch script: {output}");
        }
        
        createdVmConfig.Pid = pid;
        
        try
        {
            createdVmConfig.VmProcess = Process.GetProcessById(createdVmConfig.Pid);
        }
        catch (ArgumentException)
        {
            _logger.LogError("VM process {Pid} not found after launch", pid);
            throw new InvalidOperationException($"VM process {pid} exited immediately after launch");
        }
        
        _activeVms[vmId] = createdVmConfig;

        if (filesystemType == FilesystemType.Compiler)
        {
            _logger.LogDebug("Extracting file hashes for compiler VM {VmId}", vmId);
            var res = await QueryVmAsync<VmCompilationQuery<VmHealthCheckContent>, VmCompilerHealthCheckResponse>(
                vmId,
                new VmCompilationQuery<VmHealthCheckContent>
                {
                    Content = new VmHealthCheckContent()
                },
                ct);
            _activeVms[vmId].FileHashes = res.FileHashes;
            _logger.LogDebug("Compiler VM {VmId} ready with {HashCount} file hashes", vmId, res.FileHashes.Count);
        }
        
        return vmId;
    }

    public async Task<TResult> QueryVmAsync<T, TResult>(Guid vmId, T queryContents, CancellationToken ct = default) 
        where T : VmInputQuery 
        where TResult : VmInputResponse
    {
        if (!_activeVms.TryGetValue(vmId, out var vmConfig))
        {
            throw new InvalidOperationException($"VM {vmId} not found in active VMs");
        }
        
        if (!await _oversubManager.EnqueueResourceRequest(ResourceRequestType.Query, vmConfig.VmType, ct))
        {
            throw new VmClusterOverloadedException("Insufficient resources to execute query");
        }
        
        vmConfig.ServicedRequests++;
        
        var queryString = JsonSerializer.Serialize(queryContents);
        var queryStringEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(queryString));
        var queryProcess = ExecutorScriptHelper.CreateBashExecutionProcess(
            "/app/firecracker/query-vm.sh", 
            vmId.ToString(), 
            queryStringEncoded);
        
        queryProcess.Start();

        var timeoutPolicy = Policy.TimeoutAsync(_config.Timeouts.QueryTimeout);
        try
        {
            await timeoutPolicy.ExecuteAsync(async () => await queryProcess.WaitForExitAsync(ct));
        }
        catch (TimeoutRejectedException)
        {
            queryProcess.Kill(entireProcessTree: true);
            _logger.LogWarning("Query to VM {VmId} timed out after {Timeout}", vmId, _config.Timeouts.QueryTimeout);
            throw new VmQueryTimedOutException();
        }
        
        var path = $"/tmp/{vmId}-out.json";
        if (!File.Exists(path))
        {
            _logger.LogError("Query output file not found for VM {VmId}", vmId);
            throw new ExecutionOutputNotFoundException($"Output file not found: {path}");
        }
        
        var vmOutRaw = await File.ReadAllTextAsync(path, ct);
        _logger.LogDebug("VM {VmId} query response: {ResponseLength} chars", vmId, vmOutRaw.Length);
        
        File.Delete(path);
        
        return JsonSerializer.Deserialize<TResult>(vmOutRaw, _defaultSerializerOptions) 
               ?? throw new InvalidOperationException("Failed to deserialize VM response");
    }
    
    public bool TerminateVm(Guid vmId, bool preserveFilesystem)
    {
        _logger.LogDebug("Terminating VM {VmId}, preserveFilesystem={PreserveFilesystem}", vmId, preserveFilesystem);
        
        if (TryAddToOrphanPool(vmId))
        {
            _logger.LogDebug("VM {VmId} added to orphan pool", vmId);
            return true;
        }

        if (!_activeVms.TryRemove(vmId, out var vmData))
        {
            _logger.LogWarning("VM {VmId} not found in active VMs during termination", vmId);
            return false;
        }
        
        return TerminateVmInternal(vmData, preserveFilesystem);
    }

    private bool TerminateVmInternal(VmConfig vmData, bool preserveFilesystem)
    {
        var success = true;
        
        try
        {
            if (vmData.VmProcess is { HasExited: false })
            {
                vmData.VmProcess.Kill(entireProcessTree: true);
                _logger.LogDebug("Killed VM process {Pid}", vmData.Pid);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error killing VM process {Pid}", vmData.Pid);
            success = false;
        }

        if (!preserveFilesystem)
        {
            var fsRemoved = FilesystemPooler.RemoveFilesystemById(vmData.FilesystemId);
            if (!fsRemoved)
            {
                _logger.LogWarning("Failed to remove filesystem {FilesystemId}", vmData.FilesystemId);
                success = false;
            }
        }

        return success;
    }

    public async Task<VmLease> InspectByWatchDogAsync(VmLease lease)
    {
        if (!_activeVms.TryGetValue(lease.VmId, out var vmConfig))
        {
            throw new InvalidOperationException($"VM {lease.VmId} not found");
        }
        
        var decision = await _watchdog.InspectVmAsync(lease);
        _logger.LogDebug("Watchdog decision for VM {VmId}: {Decision}", lease.VmId, decision);
        
        return decision switch
        {
            InspectionDecision.Healthy => lease,
            InspectionDecision.RequiresReplacement => await AcquireVmAsync(vmConfig.VmType, vmConfig.VmName),
            InspectionDecision.CanBeRecycled => throw new InvalidOperationException("VM should be recycled"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private bool TryAddToOrphanPool(Guid vmId)
    {
        if (!_activeVms.TryGetValue(vmId, out var vmConfig))
            return false;
            
        if (vmConfig.ServicedRequests != 0)
            return false;
            
        if (!_orphanPool.TryGetValue(vmConfig.VmType, out var channel))
            return false;

        return channel.Writer.TryWrite(vmConfig);
    }

    public async Task<VmLease> AcquireVmAsync(FilesystemType filesystemType, string? vmName = null, CancellationToken ct = default)
    {
        var vmId = await DispatchVmAsync(filesystemType, vmName, ct);
        return new VmLease(this, vmId);
    }

    private static string GenerateName()
    {
        return $"vm-{Guid.NewGuid():N}".Substring(0, 16);
    }

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Shutting down VmLaunchManager");
        await _shutdownCts.CancelAsync();
        
        foreach (var vmId in _activeVms.Keys.ToList())
        {
            TerminateVm(vmId, preserveFilesystem: false);
        }
        
        foreach (var (type, channel) in _orphanPool)
        {
            channel.Writer.Complete();
            await foreach (var vm in channel.Reader.ReadAllAsync())
            {
                TerminateVmInternal(vm, preserveFilesystem: false);
            }
        }
        
        _shutdownCts.Dispose();
    }
}