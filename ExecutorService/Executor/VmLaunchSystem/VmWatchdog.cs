using System.Collections.Concurrent;
using System.Diagnostics;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.Types.VmLaunchTypes;

namespace ExecutorService.Executor.VmLaunchSystem;


internal enum InspectionDecision
{
    Healthy,
    RequiresReplacement,
    CanBeRecycled
}


internal class VmWatchdog(ConcurrentDictionary<Guid, VmConfig> activeVms)
{
    public async Task<InspectionDecision> InspectVmAsync(VmLease lease)
    {
        switch (activeVms[lease.VmId].VmType)
        {
            case FilesystemType.Compiler:
            {
                var res = await lease
                    .QueryAsync<VmCompilationQuery<VmHealthCheckContent>, VmCompilerHealthCheckResponse>(
                        new VmCompilationQuery<VmHealthCheckContent>
                        {
                            Content = new VmHealthCheckContent()

                        });

                var hashesMatch = activeVms[lease.VmId].FileHashes
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