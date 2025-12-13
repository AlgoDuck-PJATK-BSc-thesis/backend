using AlgoDuckShared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AlgoDuck.Modules.Problem.Shared;

public interface IExecutionStatusClient
{
    public Task ExecutionStatusUpdated(SubmitExecuteResponse executionResponse);
}

[Authorize]
public class ExecutionStatusHub : Hub<IExecutionStatusClient>
{
    public async Task SubscribeToJob(SubscriptionRequestDto requestDto)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, requestDto.JobId.ToString());
    }
}

public class SubscriptionRequestDto
{
    public required Guid JobId { get; set; }
}