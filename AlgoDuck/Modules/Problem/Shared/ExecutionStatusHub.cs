using System.Text.Json;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuck.Modules.Problem.Shared.Types;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace AlgoDuck.Modules.Problem.Shared;

public interface IExecutionStatusClient
{
    public Task ExecutionStatusUpdated(SubmitExecuteResponse executionResponse);
}

[Authorize]
public class ExecutionStatusHub : Hub<IExecutionStatusClient>
{

    private readonly IDatabase _redis;

    public ExecutionStatusHub(IDatabase redis)
    {
        _redis = redis;
    }

    public async Task<IApiResponse> SubscribeToJob(SubscriptionRequestDto requestDto)
    {
        
        var userIdResult = Context.User?.GetUserId();
        if (userIdResult is null || userIdResult.IsErr)
            return new StandardApiResponse
            {
                Status = Status.Error,
                Message = "User id not found"
            };
        
        var userId = userIdResult.AsOk;
        
        var jobDataRaw = await _redis.StringGetAsync(requestDto.JobId.ToString());

        ExecutionQueueJobDataPublic? jobData = null;
        if (!jobDataRaw.IsNullOrEmpty)
        {
            try
            {
                jobData = JsonSerializer.Deserialize<ExecutionQueueJobDataPublic>(jobDataRaw.ToString());
            }
            catch (Exception e)
            {
                jobData = null;
            }
        }

        if (jobData is not null && jobData.UserId != userId)
            return new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Cannot access another user's execution data"
            };
        
        await Groups.AddToGroupAsync(Context.ConnectionId, requestDto.JobId.ToString());

        if (jobData is null)
            return new StandardApiResponse
            {
                Message = "ok"
            };
        
        return new StandardApiResponse<ExecutionQueueJobDataPublic?>
        {
            Body = new ExecutionQueueJobDataPublic
                {
                    CachedResponses = jobData.CachedResponses,
                    JobId = jobData.JobId,
                    ProblemId = jobData.ProblemId,
                    UserId = jobData.UserId
                }
        };
    }
}


public class SubscriptionRequestDto
{
    public required Guid JobId { get; set; }
}