using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.DeleteAssistantChat;

[Authorize]
[ApiController]
[Route("[controller]")]
public class DeleteChatController(
    IDeleteChatService deleteChatService
    ) : ControllerBase
{
    public async Task<IActionResult> DeleteChat([FromBody] DeleteChatDto dto, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId.IsErr)
            return userId.ToActionResult();
        
        var chatDeleteResult  = await deleteChatService.Delete(dto, cancellationToken);
        return chatDeleteResult.ToActionResult();
    }
}

public class DeleteChatDto{
    public required string ChatName { get; set; }
    public required Guid ProblemId { get; set; }
    internal Guid UserId { get; set; }
}

public class DeleteChatDtoResult
{
    public required int MessagesDeleted { get; set; }
}