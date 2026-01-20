using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.Helpers;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.Types;
using ExecutorService.Executor.Types.Config;
using ExecutorService.Executor.Types.FilesystemPoolerTypes;
using ExecutorService.Executor.Types.OversubManagerTypes;
using ExecutorService.Executor.Types.VmLaunchTypes;
using Microsoft.Extensions.Options;
using Polly;

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
    private readonly IOptions<HealthCheckConfig> _healthCheckConfig;

    public VmLaunchManager(
        FilesystemPooler pooler,
        ILogger<VmLaunchManager> logger,
        IOptions<ExecutorConfiguration> config,
        IOptions<HealthCheckConfig> healthCheckConfig)
    {
        _pooler = pooler;
        _logger = logger;
        _config = config.Value;
        _healthCheckConfig = healthCheckConfig;
        _activeVms = new ConcurrentDictionary<Guid, VmConfig>();

        _defaultResourceAllocations = new ConcurrentDictionary<FilesystemType, VmResourceAllocation>
        {
            [FilesystemType.Executor] = new()
            {
                VcpuCount = _config.Resources.ExecutorVcpuCount,
                MemMB = _config.Resources.ExecutorMemoryMb
            },
            [FilesystemType.Compiler] = new()
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

        _watchdog = new VmWatchdog(_activeVms, healthCheckConfig);
        _oversubManager = new VmOversubManager(_activeVms, _defaultResourceAllocations);
    }

    private async Task<Guid> DispatchVmAsync(FilesystemType filesystemType, string? vmName = null,
        CancellationToken ct = default)
    {
        using var activity = new Activity("DispatchVm").Start();
        activity.SetTag("filesystem_type", filesystemType.ToString());

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
            throw new InvalidOperationException(
                $"VM launch script failed with exit code {launchProcess.ExitCode}: {stderr}");
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

        if (filesystemType != FilesystemType.Compiler) return vmId;
        
        _logger.LogDebug("Extracting file hashes for compiler VM {VmId}", vmId);
        var res = await QueryVmAsync<VmHealthCheckPayload, VmCompilerHealthCheckResponse>(
            new VmJobRequest<VmHealthCheckPayload>
            {
                JobId = vmId,
                VmId = createdVmConfig.VmId,
                Payload = new VmHealthCheckPayload
                {
                    FilesToCheck = _healthCheckConfig.Value.FileHashes
                }
            }, ct);
        _activeVms[vmId].FileHashes = res.FileHashes;
        Console.WriteLine(JsonSerializer.Serialize(res.FileHashes));
        _logger.LogDebug("Compiler VM {VmId} ready with {HashCount} file hashes", vmId, res.FileHashes.Count);

        return vmId;
    }

    public async Task<TResult> QueryVmAsync<T, TResult>(VmJobRequest<T> jobRequest, CancellationToken ct = default)
        where T : VmPayload
        where TResult : VmInputResponse
    {
        if (!_activeVms.TryGetValue(jobRequest.VmId, out var vmConfig))
        {
            throw new InvalidOperationException($"VM {jobRequest.VmId} not found in active VMs");
        }

        if (!await _oversubManager.EnqueueResourceRequest(ResourceRequestType.Query, vmConfig.VmType, ct))
        {
            throw new VmClusterOverloadedException("Insufficient resources to execute query");
        }

        vmConfig.ServicedJobs.Add(jobRequest.JobId);

        var retryPolicy = Policy
            .Handle<SocketException>()
            .Or<IOException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Math.Pow(2, attempt) * 100),
                onRetry: (exception, delay, attempt, _) =>
                {
                    Console.WriteLine(
                        $"Retry {attempt} for VM {jobRequest.VmId} after {delay.TotalMilliseconds}ms due to: {exception.Message}");
                });

        return await retryPolicy.ExecuteAsync(async (cancellationToken) =>
        {
            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            await socket.ConnectAsync(new UnixDomainSocketEndPoint(vmConfig.VsockPath!), cancellationToken);

            await using var stream = new NetworkStream(socket, ownsSocket: false);
            await using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            writer.AutoFlush = true;
            using var reader = new StreamReader(stream, new UTF8Encoding(false));

            await writer.WriteLineAsync("CONNECT 5050");

            var connectResponse = await reader.ReadLineAsync(cancellationToken);
            if (connectResponse == null || !connectResponse.StartsWith("OK "))
            {
                throw new IOException($"Vsock connect failed: {connectResponse}");
            }

            var payload = JsonSerializer.Serialize<VmPayload>(jobRequest.Payload);
            await writer.WriteAsync(payload);
            await stream.WriteAsync(new byte[] { 0x04 }, cancellationToken);

            var buffer = new StringBuilder();
            var charBuffer = new char[4096];
            while (true)
            {
                var bytesRead = await reader.ReadAsync(charBuffer, cancellationToken);
                if (bytesRead == 0) break;

                var chunk = new string(charBuffer, 0, bytesRead);
                var eotIndex = chunk.IndexOf('\x04');
                if (eotIndex >= 0)
                {
                    buffer.Append(chunk.AsSpan(0, eotIndex));
                    break;
                }

                buffer.Append(chunk);
            }

            return JsonSerializer.Deserialize<TResult>(buffer.ToString(), _defaultSerializerOptions)
                   ?? throw new InvalidOperationException("Failed to deserialize response");
        }, ct);
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

        if (vmConfig.ServicedJobs.Count != 0)
            return false;

        if (!_orphanPool.TryGetValue(vmConfig.VmType, out var channel))
            return false;

        return channel.Writer.TryWrite(vmConfig);
    }

    public async Task<VmLease> AcquireVmAsync(FilesystemType filesystemType, string? vmName = null,
        CancellationToken ct = default)
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

        foreach (var (_, channel) in _orphanPool)
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