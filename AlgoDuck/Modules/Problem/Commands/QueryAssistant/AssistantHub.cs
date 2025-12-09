using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Extensions;
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
    public async IAsyncEnumerable<ChatCompletionStreamedDto> GetAssistance(AssistantRequestDto assistantRequest)
    {
        
        assistantRequest.UserId = Context.User?.GetUserId() 
                                  ?? throw new NotFoundException("User not found");
        
        await foreach (var chatCompletionPartial in  assistantService.GetAssistanceAsync(assistantRequest))
        {
            yield return chatCompletionPartial;
        }
        
        await Clients.Caller.StreamCompleted();
    }
}