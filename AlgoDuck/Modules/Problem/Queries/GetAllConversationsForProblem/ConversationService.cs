using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.Types;

namespace AlgoDuck.Modules.Problem.Queries.GetAllConversationsForProblem;

public interface IConversationService
{
    public Task<Result<PageData<AssistanceMessageDto>, ErrorObject<string>>> GetPagedChatData(
        PageRequestDto pageRequestDto, CancellationToken cancellation = default);
}

public class ConversationService(
    IConversationRepository conversationRepository
) : IConversationService
{
    public async Task<Result<PageData<AssistanceMessageDto>, ErrorObject<string>>> GetPagedChatData(
        PageRequestDto pageRequestDto, CancellationToken cancellation = default)
    {
        return await conversationRepository.GetPagedChatData(pageRequestDto, cancellation);
    }
}