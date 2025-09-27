using ExecutorService.Executor;

namespace ExecutorService.Errors.Exceptions;

internal class VmQueryTimedOutException : Exception
{
    internal Task<VmLease>? WatchDogDecision { get; set; }
}