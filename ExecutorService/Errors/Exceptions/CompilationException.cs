namespace ExecutorService.Errors.Exceptions;

public class CompilationException(string? message) : Exception(message);
public class VmQueryException(string? message) : Exception(message);

public class JavaRuntimeError(string? message) : Exception(message);