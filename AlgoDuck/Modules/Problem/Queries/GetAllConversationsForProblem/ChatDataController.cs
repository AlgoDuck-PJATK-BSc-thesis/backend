using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Extensions;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetAllConversationsForProblem;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ChatDataController(
    IConversationService conversationService
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPagedChatDataAsync([FromQuery] int page, [FromQuery] int pageSize,
        [FromQuery] Guid chatId, CancellationToken cancellationToken)
    {
        var result = await conversationService.GetPagedChatData(new PageRequestDto
        {
            PageSize = pageSize,
            Page = page,
            ChatId = chatId,
            UserId = User.GetUserId()
        }, cancellationToken);
        return result.ToActionResult();
    }
}



[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageAuthor
{
    Assistant,
    User
}

public class PageRequestDto
{
    
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required Guid ChatId { get; set; }
    public required Guid UserId { get; set; }
}

public class ChatDto
{
    public required string ChatName { get; set; }    
    public required ICollection<AssistanceMessageDto> Messages { get; set; }
}

public class AssistanceMessageDto
{
    public required List<MessageFragmentDto> Fragments { get; set; }
    public required MessageAuthor MessageAuthor { get; set; }
    public required DateTime CreatedOn { get; set; }
}

public class MessageFragmentDto
{
    public required string Content { get; set; }
    public required FragmentType Type { get; set; }
}