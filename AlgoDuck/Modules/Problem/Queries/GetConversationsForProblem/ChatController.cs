using AlgoDuck.DAL;
using AlgoDuck.Shared.Extensions;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetConversationsForProblem;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllChatsForProblemAsync([FromQuery] Guid problemId, CancellationToken cancellationToken)
    {
        return Ok(new StandardApiResponse<ChatList>
        {
            Body = await _chatService.GetChatsForProblemAsync(new ChatListRequestDto()
            {
                ProblemId = problemId,
                UserId = User.GetUserId()
            }, cancellationToken)
        });
    }
}



public class ChatListRequestDto
{
    public required Guid ProblemId { get; set; }
    public required Guid UserId { get; set; }
}

public class ChatList
{
    public required List<ChatDetail> Chats { get; set; }
}

public class ChatDetail
{
    public required string ChatName { get; set; }
    public required Guid ChatId { get; set; }
}