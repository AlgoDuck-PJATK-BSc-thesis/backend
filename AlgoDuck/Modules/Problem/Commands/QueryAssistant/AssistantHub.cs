using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using FluentValidation;
using FluentValidation.Results;
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
    private readonly IValidator<AssistantRequestDto> _validator;

    public AssistantHub(IAssistantService assistantService, IValidator<AssistantRequestDto> validator)
    {
        _assistantService = assistantService;
        _validator = validator;
    }

    public async IAsyncEnumerable<IApiResponse> GetAssistance(AssistantRequestDto assistantRequest)
    {
        var validationResult  = await _validator.ValidateAsync(assistantRequest);
        if (!validationResult.IsValid)
            yield return new StandardApiResponse<ICollection<ValidationFailure>>
            {
                Status = Status.Error,
                Message = "validation error",
                Body = validationResult.Errors
            };
        
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