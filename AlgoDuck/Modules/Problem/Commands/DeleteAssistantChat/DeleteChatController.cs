using System.ComponentModel.DataAnnotations;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.DeleteAssistantChat;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DeleteChatController(
    IDeleteChatService deleteChatService
) : ControllerBase
{
    public async Task<IActionResult> DeleteChat([FromQuery] Guid chatId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId.IsErr)
            return userId.ToActionResult();
        
        var chatDeleteResult  = await deleteChatService.Delete(new DeleteChatDto()
        {
            ChatId = chatId,
            UserId = userId.AsT0
        }, cancellationToken);
        return chatDeleteResult.ToActionResult();
    }
}

public class DeleteChatDto{
    public required Guid ChatId { get; set; }
    internal Guid UserId { get; set; }
}

public class DeleteChatDtoResult
{
    public required int MessagesDeleted { get; set; }
}