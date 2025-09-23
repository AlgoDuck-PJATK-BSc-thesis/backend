namespace ExecutorService.Executor;


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
        var createdVmConfig = new VmConfig()
        {
            VmId = vmId,
            VmName = vmName ?? GenerateName(),
            AllocatedResources = _defaultResourceAllocations[filesystemType],
            FilesystemId = await _pooler.EnqueueFilesystemRequestAsync(filesystemType),
            GuestCid = 3,
            VmType = filesystemType,
            VsockPath = $"/tmp/{vmId}.vsock",
        };

        // ExecutorScriptHandler.CreateBashExecutionProcess();
        
        
        return vmId;
    }

    internal async Task QueryVm()
    {
        
    }

    private static string GenerateName()
    {
        return $"vm-{new Random().Next(1, 1_000_000)}"; // TODO: make this more imaginative
    }

}