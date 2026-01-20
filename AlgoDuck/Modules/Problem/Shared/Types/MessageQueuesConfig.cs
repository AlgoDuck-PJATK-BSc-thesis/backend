namespace AlgoDuck.Modules.Problem.Shared;

public class MessageQueuesConfig
{
    public required MessageQueueData Execution { get; set; }
    public required MessageQueueData Validation { get; set; }
}

public class MessageQueueData
{
    public required string Read { get; set; }
    public required string Write { get; set; }
}