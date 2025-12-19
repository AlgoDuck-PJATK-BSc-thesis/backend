using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using Microsoft.AspNetCore.Authorization;
using StackExchange.Redis;

namespace AlgoDuck.Modules.Problem.Commands.CreateProblem;

public interface IExecutionStatusClient
{
    public Task ValidationStatusUpdated(ValidationResponse validationResponse);
}

[Authorize]
public class CreateProblemUpdatesHub(
    IDatabase redis
    ) : Hub<IExecutionStatusClient>
{
    public async Task<JobData?> SubscribeToJob(SubscriptionRequestDto requestDto)
    {
        if (Context.User == null)
            return null;
        
        var userIdResult = Context.User.GetUserId();

        if (userIdResult.IsErr)
            return null;

        var jobDataRaw = await redis.StringGetAsync(requestDto.JobId.ToString());
        
        if (jobDataRaw.IsNullOrEmpty)
            return null;
        
        var jobData = JsonSerializer.Deserialize<JobData>(jobDataRaw.ToString());
        if (jobData == null || jobData.CommissioningUserId != userIdResult.AsT0)
            return null;

        await Groups.AddToGroupAsync(Context.ConnectionId, requestDto.JobId.ToString());
        
        return jobData;
    }
}

public class SubscriptionRequestDto
{
    public required Guid JobId { get; set; }
}

public class ValidationResponse
{
    public ValidationResponseStatus Status { get; set; }
}



[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValidationResponseStatus
{
    Queued,
    Pending,
    Succeeded,
    Failed
}