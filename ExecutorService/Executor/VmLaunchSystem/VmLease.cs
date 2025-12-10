using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.Types.VmLaunchTypes;

namespace ExecutorService.Executor.VmLaunchSystem;

public sealed class VmLease(VmLaunchManager manager, Guid vmId) : IDisposable
{
    internal Guid VmId => vmId;
    public async Task<TResult> QueryAsync<T, TResult>(T query) 
        where T : VmInputQuery 
        where TResult : VmInputResponse
    {
        try
        {
            var res = await manager.QueryVm<T, TResult>(vmId, query);
            return res;
        }
        catch (VmQueryTimedOutException ex)
        {
            ex.WatchDogDecision = manager.InspectByWatchDog(this);
            throw;
        } 
    }

    public void Dispose()
    {
        manager.TerminateVm(vmId, false);
    }
}