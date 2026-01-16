using System.Text.Json;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AlgoDuck.Modules.Problem.Commands.QueryAssistant;
public interface IAssistantClient
{
    public Task CompletionStatusUpdated(ChatCompletionStreamedDto completionUpdate);
    public Task StreamCompleted();
}

[Authorize]
public sealed class AssistantHub(
    IAssistantService assistantService
    ) : Hub<IAssistantClient>
{
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

        await foreach (var chatCompletionPartial in  assistantService.GetAssistanceAsync(assistantRequest))
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