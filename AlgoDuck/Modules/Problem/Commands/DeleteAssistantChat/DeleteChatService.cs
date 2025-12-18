using AlgoDuck.Shared.Http;
using OneOf.Types;

namespace AlgoDuck.Modules.Problem.Commands.DeleteAssistantChat;

public interface IDeleteChatService
{
    public Task<Result<DeleteChatDtoResult, string>> Delete(DeleteChatDto dto, CancellationToken cancellationToken = default);
}

public class DeleteChatService(
    IDeleteChatRepository deleteChatRepository
    ) : IDeleteChatService
{
    public async Task<Result<DeleteChatDtoResult, string>> Delete(DeleteChatDto dto, CancellationToken cancellationToken = default)
    {
        return await deleteChatRepository.Delete(dto, cancellationToken);
    }
}