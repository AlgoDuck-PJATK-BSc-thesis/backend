using System.Text;
using System.Text.Json;
using AlgoDuck.Modules.Problem.Shared.Types;
using AlgoDuckShared;
using RabbitMQ.Client;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertUtils;

public interface IValidationJobPublisher
{
    public Task PublishAsync(IChannel channel, UserSolutionData solutionData, CancellationToken cancellationToken);
}

public class ValidationJobPublisher : IValidationJobPublisher
{
    private const string QueueName = "problem_validation_requests";

    public async Task PublishAsync(
        IChannel channel,
        UserSolutionData solutionData,
        CancellationToken cancellationToken)
    {
        var message = new SubmitExecuteRequestRabbit
        {
            JobId = solutionData.ExecutionId,
            JavaFiles = solutionData.GetFileContents()
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: QueueName,
            mandatory: false,
            basicProperties: new BasicProperties { Persistent = true },
            body: body,
            cancellationToken: cancellationToken);
    }
}