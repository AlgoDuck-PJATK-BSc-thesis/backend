using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuckShared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace AlgoDuck.Modules.Problem.Shared;

public interface IExecutionStatusClient
{
    public Task ExecutionStatusUpdated(SubmitExecuteResponse executionResponse);
}

[Authorize]
public class ExecutionStatusHub(
    IDatabase redis
) : Hub<IExecutionStatusClient>
{
    public async Task<ExecutionJobData?> SubscribeToJob(SubscriptionRequestDto requestDto)
    {
        if (Context.User == null)
            return null;
        
        var userIdResult = Context.User.GetUserId();

        if (userIdResult.IsErr)
            return null;
        await Groups.AddToGroupAsync(Context.ConnectionId, requestDto.JobId.ToString());
        return null;
    }
}

public class SubscriptionRequestDto
{
    public required Guid JobId { get; set; }
}

public class ExecutionJobData
{
    public required Guid ProblemId { get; set; }
    public required Guid CommissioningUserId { get; set; }
    public List<SubmitExecuteResponse> CachedResponses { get; set; } = [];
}