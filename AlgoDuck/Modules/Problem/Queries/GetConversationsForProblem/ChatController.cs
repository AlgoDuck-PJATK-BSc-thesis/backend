using AlgoDuck.DAL;
using AlgoDuck.Shared.Extensions;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetConversationsForProblem;

[ApiController]
[Route("api/[controller]")]
public class ChatController(
    IChatService chatService
    ) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllChatsForProblemAsync([FromQuery] Guid problemId, CancellationToken cancellationToken)
    {
        return Ok(new StandardApiResponse<ChatList>
        {
            Body = await chatService.GetChatsForProblemAsync(new ChatListRequestDto()
            {
                ProblemId = problemId,
                UserId = User.GetUserId()
            }, cancellationToken)
        });
    }
}

public interface IChatService
{
    public Task<ChatList> GetChatsForProblemAsync(ChatListRequestDto request, CancellationToken cancellationToken);
}

public class ChatService(
    IChatRepository chatRepository
    ) : IChatService
{
    public async Task<ChatList> GetChatsForProblemAsync(ChatListRequestDto request, CancellationToken cancellationToken)
    {
        Console.WriteLine(request.ProblemId);
        Console.WriteLine(request.UserId);
        return await chatRepository.GetChatsForProblemAsync(request, cancellationToken);
    }
}

public interface IChatRepository
{
    public Task<ChatList> GetChatsForProblemAsync(ChatListRequestDto request, CancellationToken cancellationToken);
}

public class ChatRepository(
    ApplicationQueryDbContext dbContext
    ) : IChatRepository
{
    public async Task<ChatList> GetChatsForProblemAsync(ChatListRequestDto request, CancellationToken cancellationToken)
    {
        return new ChatList
        {
            Chats = await dbContext.AssistantChats
                .Where(c => c.ProblemId == request.ProblemId && c.UserId == request.UserId)
                .Select(c => new ChatDetail
                {
                    ChatName = c.Name
                }).ToListAsync(cancellationToken: cancellationToken) 
        };
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
}