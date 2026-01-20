using AlgoDuck.Shared.Http;
using OneOf.Types;

namespace AlgoDuck.Modules.Problem.Commands.DeleteAssistantChat;

public interface IDeleteChatService
{
    public Task<Result<DeleteChatDtoResult, ErrorObject<string>>> Delete(DeleteChatDto dto,
        CancellationToken cancellationToken = default);
}

public class DeleteChatService : IDeleteChatService
{
    private readonly IDeleteChatRepository _deleteChatRepository;

    public DeleteChatService(IDeleteChatRepository deleteChatRepository)
    {
        _deleteChatRepository = deleteChatRepository;
    }

    public async Task<Result<DeleteChatDtoResult, ErrorObject<string>>> Delete(DeleteChatDto dto,
        CancellationToken cancellationToken = default)
    {
        return await _deleteChatRepository.Delete(dto, cancellationToken);
    }
}