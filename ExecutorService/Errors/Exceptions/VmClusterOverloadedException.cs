namespace ExecutorService.Errors.Exceptions;

public class VmClusterOverloadedException(string? message = "") : Exception(message);