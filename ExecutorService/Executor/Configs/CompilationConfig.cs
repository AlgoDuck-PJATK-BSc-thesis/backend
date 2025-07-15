namespace ExecutorService.Executor.Configs;

public static class CompilationConfig
{
    public const short BaselinePort = 5137; // TODO probably read these from some env file
    public const int DefaultContainerCount = 5;
    public const int DefaultThreadCount = 1;
}
