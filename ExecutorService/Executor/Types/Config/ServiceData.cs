namespace ExecutorService.Executor.Types.Config;

public class ServiceData
{
    public required string ServiceName { get; set; }
    public required string RequestQueueName { get; set; }
    public required string ResponseQueueName { get; set; }
}
