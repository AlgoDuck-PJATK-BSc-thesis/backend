using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.Helpers;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.Types.VmLaunchTypes;
using Microsoft.OpenApi.Extensions;
using Polly;
using Polly.Timeout;

namespace ExecutorService.Executor.VmLaunchSystem;

public class VmLaunchManager
{
    private readonly FilesystemPooler _pooler;
    
    private readonly JsonSerializerOptions _defaultSerializerOptions = new() 
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly ConcurrentDictionary<FilesystemType, VmResourceAllocation> _defaultResourceAllocations = new()
    {
        [FilesystemType.Executor] = new VmResourceAllocation
        {
            VcpuCount = 1,
            MemMB = 256
        },
        [FilesystemType.Compiler] = new VmResourceAllocation
        {
            VcpuCount = 2,
            MemMB = 2048,
        }
    };

    private readonly ConcurrentDictionary<Guid, VmConfig> _activeVms;
    private readonly VmWatchdog _watchdog;
    private readonly VmOversubManager _oversubManager;

    private int _nextGuestCid = 3; // 4 byte uint. (0 - loopback, 1 - general vsock, 2 - hypervisor) reserved so start at 3 and go from there

    private readonly ConcurrentDictionary<FilesystemType, Channel<VmConfig>> _orphanPool = new()
    {
        [FilesystemType.Executor] = Channel.CreateBounded<VmConfig>(new BoundedChannelOptions(5)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
        })
    };

    public VmLaunchManager(FilesystemPooler pooler)
    {
        _pooler = pooler;
        _activeVms = [];
        _watchdog = new VmWatchdog(_activeVms);
        _oversubManager = new VmOversubManager(_activeVms, _defaultResourceAllocations);
    }

    private async Task<Guid> DispatchVm(FilesystemType filesystemType, string? vmName = null)
    {
        Console.WriteLine($"Dispatching {filesystemType.GetDisplayName()}: {DateTime.UtcNow}");
        if (_orphanPool.TryGetValue(filesystemType, out var value))
        {
            Console.WriteLine(value.Reader.Count);
            if (value.Reader.TryRead(out var config))
            {
                Console.WriteLine("Got from orphan pool");
                return config.VmId;                
            }
        }
        
        var vmId = Guid.NewGuid();

        var createdVmConfig = new VmConfig
        {
            VmId = vmId,
            VmName = vmName ?? GenerateName(),
            AllocatedResources = _defaultResourceAllocations[filesystemType],
            FilesystemId = await _pooler.EnqueueFilesystemRequestAsync(filesystemType),
            GuestCid = _nextGuestCid++,
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

        Console.WriteLine($"checking if enough for launch {DateTime.Now}");
        if (!await _oversubManager.EnqueueResourceRequest(ResourceRequestType.Spawn, filesystemType))
            throw new VmClusterOverloadedException(); // TODO: should probably be something else
        Console.WriteLine($"checked {DateTime.Now}");

        launchProcess.Start();
        await launchProcess.WaitForExitAsync();
        Console.WriteLine($"launched {DateTime.Now}");

        
        
        var output = await launchProcess.StandardOutput.ReadToEndAsync();
        createdVmConfig.Pid = int.Parse(output.Trim());
        createdVmConfig.VmProcess = Process.GetProcessById(createdVmConfig.Pid);
        _activeVms[vmId] = createdVmConfig;

        Console.WriteLine($"active {DateTime.Now}");
        
        
        if (filesystemType != FilesystemType.Compiler) return vmId;
        Console.WriteLine($"Compiler {vmId}: READY{Environment.NewLine}Extracting file hashes");
        // TODO: This is stupid, we're getting baseline hashes from a static image by querying the vm after launch, should be pre-computed
        var res = await QueryVm<VmCompilationQuery<VmHealthCheckContent>, VmCompilerHealthCheckResponse>(vmId,
            new VmCompilationQuery<VmHealthCheckContent>
            {
                Content = new VmHealthCheckContent()
            });
        _activeVms[vmId].FileHashes = res.FileHashes;
        Console.WriteLine($"Compiler {vmId}: File hashes ready");
        return vmId;
    }

    public async Task<TResult> QueryVm<T, TResult>(Guid vmId, T queryContents) where T: VmInputQuery where TResult: VmInputResponse
    {
        if (!await _oversubManager.EnqueueResourceRequest(ResourceRequestType.Query, _activeVms[vmId].VmType))
            throw new VmClusterOverloadedException(); // TODO: should probably be something else
        
        _activeVms[vmId].ServicedRequests++;
        var queryString = JsonSerializer.Serialize(queryContents);
        var queryStringEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(queryString));
        var queryProcess = ExecutorScriptHelper.CreateBashExecutionProcess("/app/firecracker/query-vm.sh", vmId.ToString(), queryStringEncoded);
        queryProcess.Start();

        var vmQueryTimeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(15));
        try
        {
            await vmQueryTimeoutPolicy.ExecuteAsync(async () => await queryProcess.WaitForExitAsync());
        }
        catch (TimeoutRejectedException)
        {
            throw new VmQueryTimedOutException();
        }
        
        var path = $"/tmp/{vmId}-out.json";
        if (!File.Exists(path)) throw new ExecutionOutputNotFoundException();
        
        var vmOutRaw = await File.ReadAllTextAsync(path);
        Console.WriteLine(vmOutRaw);
        File.Delete(path);
        return JsonSerializer.Deserialize<TResult>(vmOutRaw, _defaultSerializerOptions)!;
    }
    
    public bool TerminateVm(Guid vmId, bool withFreeze)
    {
        Console.WriteLine("terminating vm");
        if (TryAddToOrphanPool(vmId)) return false;

        if (!_activeVms.Remove(vmId, out var vmData)) return false;
        try
        {
            var fcProcess = Process.GetProcessById(vmData.Pid);
            fcProcess.Kill();
            var vmFilesystemRemoved = true;
            if (!withFreeze)
            {
                vmFilesystemRemoved = FilesystemPooler.RemoveFilesystemById(vmData.FilesystemId);
            }
            return fcProcess.HasExited && vmFilesystemRemoved;
        }
        catch (ArgumentException)
        {
            return !withFreeze || FilesystemPooler.RemoveFilesystemById(vmData.FilesystemId);
        }
    }

    public async Task<VmLease> InspectByWatchDog(VmLease lease)
    {
        switch (await _watchdog.InspectVmAsync(lease))
        {
            case InspectionDecision.Healthy:
                return lease;
            case InspectionDecision.RequiresReplacement:
                return await AcquireVmAsync(_activeVms[lease.VmId].VmType, _activeVms[lease.VmId].VmName);
            case InspectionDecision.CanBeRecycled:
                TerminateVm(lease.VmId, false);
                return lease;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool TryAddToOrphanPool(Guid vmId)
    {
        var vmConfig = _activeVms[vmId];
        // Console.WriteLine("trying orphan pool");
        if (vmConfig.ServicedRequests != 0 || !_orphanPool.TryGetValue(vmConfig.VmType, out var value)) return false;
        // Console.WriteLine("trying orphan pool 2");
        return value.Writer.TryWrite(vmConfig);
    }

    public async Task<VmLease> AcquireVmAsync(FilesystemType filesystemType, string? vmName = null)
    {
        var vmId = await DispatchVm(filesystemType, vmName);
        return new VmLease(this, vmId);
    }

    private static string GenerateName()
    {
        return $"vm-{new Random().Next(1, 1_000_000)}"; // TODO: make this more imaginative
    }
}