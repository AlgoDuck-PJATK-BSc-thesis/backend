using System.Collections.Concurrent;
using System.Threading.Channels;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.Types.VmLaunchTypes;
using Microsoft.OpenApi.Extensions;

namespace ExecutorService.Executor.VmLaunchSystem;

internal class VmOversubManager
{
    private readonly IReadOnlyDictionary<Guid, VmConfig> _activeVms;
    
    private const double MaxAllowedVcpuOversub = 1.5;

    private readonly TotalVmResourceAllocation _maxResourceAllocation;
    private readonly TotalVmResourceAllocation _maxResourceUsage;

    private readonly Channel<ResourceRequest> _resourceRequests;
    private readonly ConcurrentDictionary<FilesystemType, VmResourceAllocation> _defaultResourceAllocations;
    
    public VmOversubManager(IReadOnlyDictionary<Guid, VmConfig> activeVms, ConcurrentDictionary<FilesystemType, VmResourceAllocation> defaultResourceAllocations)
    {
        _activeVms = activeVms;
        _defaultResourceAllocations = defaultResourceAllocations;
        _resourceRequests = Channel.CreateUnbounded<ResourceRequest>();
        
        _maxResourceUsage = new TotalVmResourceAllocation
        {
            MemMb = int.Parse(Environment.GetEnvironmentVariable("CLUSTER_TOTAL_MEM_MB") ?? "8192"),
            VcpuCount = int.Parse(Environment.GetEnvironmentVariable("CLUSTER_TOTAL_VCPU") ?? "8"),
        };
        
        _maxResourceAllocation = new TotalVmResourceAllocation
        {
            MemMb = _maxResourceUsage.MemMb,
            VcpuCount = (int)Math.Floor(_maxResourceUsage.VcpuCount * MaxAllowedVcpuOversub)
        };
        var cts = new CancellationTokenSource();
        Task.Run(() => WatchdogThreadAsync(cts.Token));
    }

    private const long ResourcePollingFrequencyMillis = 100;

    public async Task<bool> EnqueueResourceRequest(ResourceRequestType requestType, FilesystemType vmType)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var request = new ResourceRequest
        {
            CancellationToken = cts.Token,
            Decision = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously),
            RequiredAllocation = vmType,
            Type = requestType,
        };

        Console.WriteLine("starting write");
        await _resourceRequests.Writer.WriteAsync(request, cts.Token);
        Console.WriteLine("end write");
        return await request.Decision.Task;
    }

    private async Task ValidateRequest(ResourceRequest request)
    {
        var requestConfirmed = request.Type switch
        {
            ResourceRequestType.Spawn => await CheckIfResourcesSufficientForQueryAsync(request, CalculateTotalClusterResourceAllocation, _maxResourceAllocation),
            ResourceRequestType.Query => await CheckIfResourcesSufficientForQueryAsync(request, CalculateTotalClusterResourceUsage, _maxResourceUsage),
            _ => throw new ArgumentOutOfRangeException()
        };
        Console.WriteLine($"Decision {request.Type.GetDisplayName()}: {requestConfirmed}");
        request.Decision.SetResult(requestConfirmed);
    }
    
    private async Task<bool> CheckIfResourcesSufficientForQueryAsync(ResourceRequest request, Func<TotalVmResourceAllocation> getCurrentValue, TotalVmResourceAllocation maxTarget)
    {
        // Console.WriteLine("Checking if sufficent for query");
        var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(ResourcePollingFrequencyMillis));
        while (await periodicTimer.WaitForNextTickAsync(request.CancellationToken))
        {
            request.CancellationToken.ThrowIfCancellationRequested();
            Console.WriteLine("calculating currVal");
            var currentValue = getCurrentValue();

            Console.WriteLine($"Memory: {currentValue.MemMb}");
            Console.WriteLine($"Cores: {currentValue.VcpuCount}");
            if (currentValue.VcpuCount + _defaultResourceAllocations[request.RequiredAllocation].VcpuCount <=
                maxTarget.VcpuCount && currentValue.MemMb + _defaultResourceAllocations[request.RequiredAllocation].MemMB <=
                maxTarget.MemMb)
            {
                return true;
            }
        }

        throw new VmClusterOverloadedException();
    }
    
    private async Task WatchdogThreadAsync(CancellationToken cancellationToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(ResourcePollingFrequencyMillis));
    
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            // Console.WriteLine("polling tick");
            foreach (var vmConfig in _activeVms.Values)
            {
                await UpdateProcessStatistics(vmConfig, cancellationToken);
            }
            // Console.WriteLine("stat update");
            // Console.WriteLine(_resourceRequests.Reader.Count);
            while (_resourceRequests.Reader.TryRead(out var request))
            {
                Console.WriteLine("servicing request");
            
                await ValidateRequest(request);
            }
        }
    }

    private TotalVmResourceAllocation CalculateTotalClusterResourceAllocation()
    {
        return _activeVms.Values.Select(vmConf => new TotalVmResourceAllocation
        {
            MemMb = vmConf.AllocatedResources!.MemMB,
            VcpuCount = vmConf.AllocatedResources.VcpuCount,
        }).Aggregate(
            new TotalVmResourceAllocation { VcpuCount = 0, MemMb = 0 }, 
            (acc, vm) =>
        {
            acc.VcpuCount += vm.VcpuCount;
            acc.MemMb += vm.MemMb;
            return acc;
        });
    }
    
    private TotalVmResourceAllocation CalculateTotalClusterResourceUsage()
    {
        var calculateTotalClusterResourceUsage = _activeVms.Values.Select(vmConf => new TotalVmResourceAllocation
        {
            MemMb = vmConf.UsedResources.WorkingMemBytes / (1024 * 1024),
            VcpuCount = vmConf.UsedResources.CpuTime,
        }).Aggregate(
            new TotalVmResourceAllocation { VcpuCount = 0, MemMb = 0 }, 
            (acc, vm) =>
            {
                acc.VcpuCount += vm.VcpuCount;
                acc.MemMb += vm.MemMb;
                return acc;
            });
        Console.WriteLine($"Cluster cpu usage {calculateTotalClusterResourceUsage.VcpuCount}");
        return calculateTotalClusterResourceUsage;
    }
    
    private static async Task UpdateProcessStatistics(VmConfig vmConfig, CancellationToken token)
    { 
        token.ThrowIfCancellationRequested();
        var vmProcess = vmConfig.VmProcess!;

        vmProcess.Refresh();
        var currentProcessorTime = vmProcess.TotalProcessorTime;

        if (vmConfig.UsedResources.LastCpuTime > TimeSpan.Zero)
        {
            var cpuDelta = (currentProcessorTime - vmConfig.UsedResources.LastCpuTime).TotalMilliseconds;
            vmConfig.UsedResources.CpuTime = cpuDelta / ResourcePollingFrequencyMillis / vmConfig.AllocatedResources!.VcpuCount;
        }

        vmConfig.UsedResources.LastCpuTime = currentProcessorTime;
    
        vmConfig.UsedResources.WorkingMemBytes = vmProcess.WorkingSet64;
        
        var ioContent = await File.ReadAllTextAsync($"/proc/{vmProcess.Id}/io", token);
        var ioLines = ioContent.Split('\n');
        vmConfig.UsedResources.IoData.BytesRead =
            long.Parse(ioLines.First(l => l.StartsWith("read_bytes:")).Split(':')[1].Trim());
        vmConfig.UsedResources.IoData.BytesWritten =
            long.Parse(ioLines.First(l => l.StartsWith("write_bytes:")).Split(':')[1].Trim());
    }
}