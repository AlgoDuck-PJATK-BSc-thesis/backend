using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;

namespace AlgoDuck.Modules.Problem.Commands.QueryAssistant;
public interface IAssistantClient
{
    public Task CompletionStatusUpdated(ChatCompletionStreamedDto completionUpdate);
    public Task StreamCompleted();
}

[Authorize]
[EnableRateLimiting("Assistant")]
public sealed class AssistantHub : Hub<IAssistantClient>
{
    private readonly IAssistantService _assistantService;

    public AssistantHub(IAssistantService assistantService)
    {
        _assistantService = assistantService;
    }

    public async IAsyncEnumerable<IApiResponse> GetAssistance(AssistantRequestDto assistantRequest)
    {
        var userId = Context.User?.GetUserId();
        if (userId == null || userId.IsErr)
        {
            yield return new StandardApiResponse
            {
                Status = Status.Error,
                Message = userId!.AsErr!.Body,
            };
            yield break;
        }

        assistantRequest.UserId = userId.AsOk;

        await foreach (var chatCompletionPartial in  _assistantService.GetAssistanceAsync(assistantRequest))
        {
            if (chatCompletionPartial.IsErr)
            {
                yield return new StandardApiResponse
                {
                    Status = Status.Error,
                    Message = chatCompletionPartial.AsErr!.Body,
                };
                yield break;
            }
            yield return new StandardApiResponse<ChatCompletionStreamedDto>
            {
                Body = chatCompletionPartial.AsOk
            };
        }
        await Clients.Caller.StreamCompleted();
    }
}