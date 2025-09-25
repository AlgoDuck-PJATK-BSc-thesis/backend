using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using ExecutorService.Errors.Exceptions;

namespace ExecutorService.Executor;

public abstract class VmInputQuery;

public abstract class VmInputResponse;

public class VmCompilationQueryContent
{
    public string SrcCodeB64 { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public Guid ExecutionId { get; set; }
}
public class VmCompilationQuery : VmInputQuery
{
    public string Endpoint { get; set; } = "HealthCheck";
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public VmCompilationQueryContent? Content { get; set; }
    public string Ctype { get; set; } = "application/json";
}

public class VmCompilationResponse : VmInputResponse
{
    public string Entrypoint { get; set; } = string.Empty;
    public Dictionary<string, string> GeneratedClassFiles { get; set; } = [];
}

public class VmExecutionQuery : VmInputQuery
{
    public VmExecutionQuery(VmCompilationResponse compilationResponse)
    {
        Entrypoint = compilationResponse.Entrypoint;
        GeneratedClassFiles = compilationResponse.GeneratedClassFiles;
    }
    public string Entrypoint { get; set; }
    public Dictionary<string, string> GeneratedClassFiles { get; set; }
}

public class VmExecutionResponse : VmInputResponse
{
    public string Out { get; set; } = string.Empty;
    public string Err { get; set; } = string.Empty;
}

internal sealed class VmLease(VmLaunchManager manager, Guid vmId) : IDisposable
{
    public async Task<TResult> QueryAsync<T, TResult>(T query) 
        where T : VmInputQuery 
        where TResult : VmInputResponse
    {
        return await manager.QueryVm<T, TResult>(vmId, query);
    }

    public void Dispose()
    {
        if (!manager.TryAddToOrphanPool(vmId))
        {
            manager.TerminateVm(vmId, false);
        }
    }
}

internal class VmLaunchManager
{
    private class VmResourceAllocation
    {
        internal int VcpuCount { get; set; }
        internal int MemMB { get; set; }
        internal bool Smt { get; set; } = false;
    }
    private class VmConfig
    {
        internal Guid VmId { get; set; }
        internal string? VmName { get; set; }
        internal FilesystemType VmType { get; set; }
        internal VmResourceAllocation? AllocatedResources { get; set; }
        internal Guid FilesystemId { get; set; }
        internal int GuestCid { get; set; }
        internal string? VsockPath { get; set; }
        internal int Pid { get; set; }
        internal int ServicedRequests { get; set; }
    }
    
    private readonly FilesystemPooler _pooler;
    private readonly Dictionary<Guid, VmConfig> _activeVms = [];

    private readonly Dictionary<FilesystemType, VmResourceAllocation> _defaultResourceAllocations;

    private int _nextGuestCid = 3; // 4 byte uint. (0 - loopback, 1 - general vsock, 2 - hypervisor) reserved so start at 3 and go from there

    private readonly Dictionary<FilesystemType, Channel<VmConfig>> _orphanPool;
    
    public VmLaunchManager(FilesystemPooler pooler)
    {
        _pooler = pooler;
        _defaultResourceAllocations = new Dictionary<FilesystemType, VmResourceAllocation>()
        {
            [FilesystemType.Executor] = new()
            {
                VcpuCount = 1,
                MemMB = 256
            },
            [FilesystemType.Compiler] = new()
            {
                VcpuCount = 2,
                MemMB = 2048,
            }
        };
        _orphanPool = new Dictionary<FilesystemType, Channel<VmConfig>>
        {
            [FilesystemType.Executor] = Channel.CreateBounded<VmConfig>(new BoundedChannelOptions(5)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
            })
        };
    }

    private async Task<Guid> DispatchVm(FilesystemType filesystemType, string? vmName = null)
    {
        if (_orphanPool.TryGetValue(filesystemType, out var value) && value.Reader.TryRead(out var config))
        {
            return config.VmId;
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
        
        var launchProcess = ExecutorScriptHandler.CreateBashExecutionProcess(
            launchScriptPath,
            createdVmConfig.VmId.ToString(),
            createdVmConfig.GuestCid.ToString(), 
            createdVmConfig.FilesystemId.ToString(),
            createdVmConfig.AllocatedResources.VcpuCount.ToString(),
            createdVmConfig.AllocatedResources.MemMB.ToString(),
            createdVmConfig.AllocatedResources.Smt.ToString().ToLowerInvariant()
            );
        

        launchProcess.Start();
        await launchProcess.WaitForExitAsync();

        var output = await launchProcess.StandardOutput.ReadToEndAsync();
        createdVmConfig.Pid = int.Parse(output.Split('\n').Last().Trim());

        _activeVms[vmId] = createdVmConfig;

        return vmId;
    }

    internal async Task<TResult> QueryVm<T, TResult>(Guid vmId, T queryContents) where T: VmInputQuery where TResult: VmInputResponse
    {
        _activeVms[vmId].ServicedRequests++;
        var queryString = JsonSerializer.Serialize(queryContents);
        var queryStringEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(queryString));
        var queryProcess = ExecutorScriptHandler.CreateBashExecutionProcess("/app/firecracker/query-vm.sh", vmId.ToString(), queryStringEncoded);
        queryProcess.Start();
        await queryProcess.WaitForExitAsync();
        
        var path = $"/tmp/{vmId}-out.json";
        if (!File.Exists(path)) throw new ExecutionOutputNotFoundException();
        
        var vmOutRaw = await File.ReadAllTextAsync(path);
        File.Delete(path);
        
        var vmOut = JsonSerializer.Deserialize<TResult>(vmOutRaw, new JsonSerializerOptions // TODO: Move this to like shared.core defaultJsonSerializer.Deserialize()
        {
            PropertyNameCaseInsensitive = true,
        });
        return vmOut!;
    }

    internal bool TryAddToOrphanPool(Guid vmId)
    {
        var vmConfig = _activeVms[vmId];
        if (vmConfig.ServicedRequests != 0 || !_orphanPool.TryGetValue(vmConfig.VmType, out var value)) return false;
        return value.Writer.TryWrite(vmConfig);
    }

    internal bool TerminateVm(Guid vmId, bool withFreeze)
    {
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
    
    internal async Task<VmLease> AcquireVmAsync(FilesystemType filesystemType, string? vmName = null)
    {
        var vmId = await DispatchVm(filesystemType, vmName);
        return new VmLease(this, vmId);
    }

    private static string GenerateName()
    {
        return $"vm-{new Random().Next(1, 1_000_000)}"; // TODO: make this more imaginative
    }

}