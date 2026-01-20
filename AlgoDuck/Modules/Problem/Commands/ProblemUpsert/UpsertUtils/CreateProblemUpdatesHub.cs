using System.Text.Json;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertUtils;

public interface IExecutionStatusClient
{
    public Task ValidationStatusUpdated(ValidationResponse validationResponse);
}

[Authorize]
public class CreateProblemUpdatesHub(
    IDatabase redis
    ) : Hub<IExecutionStatusClient>
{
    public async Task<IApiResponse?> SubscribeToJob(SubscriptionRequestDto requestDto)
    {
        if (Context.User == null)
            return null;
        
        var userIdResult = Context.User.GetUserId();

        if (userIdResult.IsErr)
            return null;

        var jobDataRaw = await redis.StringGetAsync(requestDto.JobId.ToString());
        
        if (jobDataRaw.IsNullOrEmpty)
            return null;
        
        var jobData = JsonSerializer.Deserialize<JobData<UpsertProblemDto?>>(jobDataRaw.ToString());
        if (jobData == null || jobData.CommissioningUserId != userIdResult.AsT0)
            return null;

        await Groups.AddToGroupAsync(Context.ConnectionId, requestDto.JobId.ToString());

        return new StandardApiResponse<ICollection<ValidationResponse>>
        {
            Body = jobData.CachedResponses
        };
    }
}






