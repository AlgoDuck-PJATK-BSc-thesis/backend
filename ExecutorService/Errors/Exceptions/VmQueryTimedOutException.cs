using ExecutorService.Executor.VmLaunchSystem;

namespace ExecutorService.Errors.Exceptions;

internal class VmQueryTimedOutException : Exception
{
    // TODO: I don't know if this is protocol, since it uses the exception's flow up the call stack for control flow. It does make for clean handling though.
    internal Task<VmLease>? WatchDogDecision { get; set; }
}