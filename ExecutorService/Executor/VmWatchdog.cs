namespace ExecutorService.Executor;

internal class VmWatchdog(VmLaunchManager launchManager)
{
    internal async Task<VmLease> InspectVm(VmLease lease, Dictionary<string, string> fileHashes)
    {
        var res = await lease.QueryAsync<VmCompilationQuery<VmHealthCheckContent>, VmCompilerHealthCheckResponse>(
            new VmCompilationQuery<VmHealthCheckContent>
            {
                Content = new VmHealthCheckContent()
            });

        var doFileHashesMatch = fileHashes.All(keyValuePair => res.FileHashes[keyValuePair.Key] == keyValuePair.Value);

        if (doFileHashesMatch)
        {
            return lease;
        }
        launchManager.TerminateVm(lease.VmId, true);
        return await launchManager.AcquireVmAsync(FilesystemType.Compiler);
    }
}