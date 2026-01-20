using AlgoDuck.DAL;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetConversationsForProblem;

public interface IChatRepository
{
    public Task<ChatList> GetChatsForProblemAsync(ChatListRequestDto request, CancellationToken cancellationToken);
}


public class ChatRepository : IChatRepository
{
    private readonly ApplicationQueryDbContext _dbContext;

    public ChatRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ChatList> GetChatsForProblemAsync(ChatListRequestDto request, CancellationToken cancellationToken)
    {
        return new ChatList
        {
            Chats = await _dbContext.AssistantChats
                .Include(c => c.Messages)
                .Where(c => c.ProblemId == request.ProblemId && c.UserId == request.UserId && c.Messages.Count > 0)
                .Select(c => new ChatDetail
                {
                    ChatName = c.Name,
                    ChatId = c.Id
                }).ToListAsync(cancellationToken: cancellationToken) 
        };
    }
}