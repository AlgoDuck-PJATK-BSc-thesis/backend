namespace ExecutorService.Errors.Exceptions;

public class CompilationHandlerChannelReadException(string? message = "") : Exception(message);

public class ExecutionOutputNotFoundException(string? message = "") : Exception(message);