namespace ExecutorService.Executor.Types.Config;

public class BasicQosOptions
{
    public uint PrefetchSize { get; set; } = 0;
    public ushort PrefetchCount { get; set; } = 10;
    public bool Global { get; set; } = false;
}