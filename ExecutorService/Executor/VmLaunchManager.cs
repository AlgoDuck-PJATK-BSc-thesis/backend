using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Extensions;

namespace ExecutorService.Executor;

internal abstract class VmInputQuery;

internal abstract class VmInputResponse;

internal class VmCompilationQueryContent
{
    internal string SrcCodeB64 { get; set; } = string.Empty;
    internal string ClassName { get; set; } = string.Empty;
    internal Guid ExecutionId { get; set; }
}
internal class VmCompilationQuery : VmInputQuery
{
    internal string Endpoint { get; set; } = "HealthCheck";
    internal HttpMethod Method { get; set; } = HttpMethod.Get;
    internal VmCompilationQueryContent? Content { get; set; }
    internal string Ctype { get; set; } = "application/json";
}

internal class VmCompilationResponse : VmInputResponse
{
    internal string Entrypoint { get; set; } = string.Empty;
    internal Dictionary<string, string> GeneratedClassFiles = [];
}

// TODO: Add copy constructor
internal class VmExecutionQuery : VmInputQuery
{
    internal string Entrypoint { get; set; } = string.Empty;
    internal Dictionary<string, string> GeneratedClassFiles = [];
}

internal class VmExecutionResponse : VmInputResponse
{
    internal string Out { get; set; } = string.Empty;
    internal string Err { get; set; } = string.Empty;
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
    }
    
    private readonly FilesystemPooler _pooler;
    private readonly Dictionary<Guid, VmConfig> _activeVms = [];

    private readonly Dictionary<FilesystemType, VmResourceAllocation> _defaultResourceAllocations;


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
    }

    internal async Task<Guid> DispatchVm(FilesystemType filesystemType, string? vmName = null)
    {
        var vmId = Guid.NewGuid();
        var createdVmConfig = new VmConfig
        {
            VmId = vmId,
            VmName = vmName ?? GenerateName(),
            AllocatedResources = _defaultResourceAllocations[filesystemType],
            FilesystemId = await _pooler.EnqueueFilesystemRequestAsync(filesystemType),
            GuestCid = 17,
            VmType = filesystemType,
            VsockPath = $"/tmp/{vmId}.vsock",
        };

        const string launchScriptPath = "/app/firecracker/launch-vm.sh";
        
        var launchProcess = ExecutorScriptHandler.CreateBashExecutionProcess(
            launchScriptPath,
            createdVmConfig.VmId.ToString(),
            createdVmConfig.GuestCid.ToString(), 
            createdVmConfig.FilesystemId.ToString(),
            createdVmConfig.AllocatedResources.VcpuCount.ToString(),
            createdVmConfig.AllocatedResources.MemMB.ToString(),
            createdVmConfig.AllocatedResources.Smt.ToString()
            );

        launchProcess.Start();
        await launchProcess.WaitForExitAsync();

        var output = await launchProcess.StandardOutput.ReadToEndAsync();
        createdVmConfig.Pid = int.Parse(output.Trim());

        _activeVms[vmId] = createdVmConfig;
        
        return vmId;
    }

    internal async Task<TResult> QueryVm<T, TResult>(Guid vmId, T queryContents) where T: VmInputQuery where TResult: VmInputResponse
    {
        var queryString = JsonSerializer.Serialize(queryContents);
        var queryProcess = ExecutorScriptHandler.CreateBashExecutionProcess("/app/firecracker/query-vm.sh", vmId.ToString(), queryString);
        queryProcess.Start();
        await queryProcess.WaitForExitAsync();

        var vmOutRaw = await File.ReadAllTextAsync($"/tmp/{vmId}-out.json");
        var vmOut = JsonSerializer.Deserialize<TResult>(vmOutRaw);
        return vmOut!;
    }

    private static string GenerateName()
    {
        return $"vm-{new Random().Next(1, 1_000_000)}"; // TODO: make this more imaginative
    }

}