using System.Collections.Concurrent;
using System.Diagnostics;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.Types;
using ExecutorService.Executor.Types.Config;
using ExecutorService.Executor.Types.FilesystemPoolerTypes;
using ExecutorService.Executor.Types.VmLaunchTypes;
using Microsoft.Extensions.Options;

namespace ExecutorService.Executor.VmLaunchSystem;


internal enum InspectionDecision
{
    Healthy,
    RequiresReplacement,
    CanBeRecycled
}


internal class VmWatchdog
{
    private readonly ConcurrentDictionary<Guid, VmConfig> _activeVms;
    private readonly IOptions<HealthCheckConfig> _healthCheckConfig;

    public VmWatchdog(ConcurrentDictionary<Guid, VmConfig> activeVms, IOptions<HealthCheckConfig> healthCheckConfig)
    {
        _activeVms = activeVms;
        _healthCheckConfig = healthCheckConfig;
    }

    public async Task<InspectionDecision> InspectVmAsync(VmLease lease)
    {
        switch (_activeVms[lease.VmId].VmType)
        {
            case FilesystemType.Compiler:
            {
                var res = await lease.QueryAsync<VmHealthCheckPayload, VmCompilerHealthCheckResponse>(
                    new VmJobRequestInterface<VmHealthCheckPayload>
                    {
                        JobId = Guid.NewGuid(),
                        Payload = new VmHealthCheckPayload
                        {
                            FilesToCheck = _healthCheckConfig.Value.FileHashes
                        }
                    });
                
                var hashesMatch = _activeVms[lease.VmId].FileHashes
                    .All(keyValuePair => res.FileHashes[keyValuePair.Key] == keyValuePair.Value);
                return hashesMatch ? InspectionDecision.Healthy : InspectionDecision.RequiresReplacement;
            }
        
            case FilesystemType.Executor:
                return InspectionDecision.CanBeRecycled;
                
                default:
                throw new ArgumentOutOfRangeException();
        }
    }
}