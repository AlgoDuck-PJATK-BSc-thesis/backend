using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.Types.VmLaunchTypes;

namespace ExecutorService.Executor.VmLaunchSystem;

public sealed class VmLease(VmLaunchManager manager, Guid vmId) : IDisposable
{
    internal Guid VmId => vmId;
    public async Task<TResult> QueryAsync<T, TResult>(VmJobRequestInterface<T> query) 
        where T : VmPayload 
        where TResult : VmInputResponse
    {
        try
        {
            var res = await manager.QueryVmAsync<T, TResult>(new VmJobRequest<T>
            {
                JobId = query.JobId,
                Payload = query.Payload,
                VmId =  vmId
            });
            return res;
        }
        catch (VmQueryTimedOutException ex)
        {
            ex.WatchDogDecision = manager.InspectByWatchDogAsync(this);
            throw;
        } 
    }

    public void Dispose()
    {
        Console.WriteLine($"info: ExecutorService.Executor.VmLaunchSystem.VmLease[0]\n       {DateTime.Now} | Disposing VmLease for VM: {VmId}");
        manager.TerminateVm(vmId, false);
    }
}