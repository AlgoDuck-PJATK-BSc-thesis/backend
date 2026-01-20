namespace AlgoDuck.Modules.Problem.Queries.GetConversationsForProblem;

public interface IChatService
{
    public Task<ChatList> GetChatsForProblemAsync(ChatListRequestDto request, CancellationToken cancellationToken);
}

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;

    public ChatService(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }

    public async Task<ChatList> GetChatsForProblemAsync(ChatListRequestDto request, CancellationToken cancellationToken)
    {
        return await _chatRepository.GetChatsForProblemAsync(request, cancellationToken);
    }
}