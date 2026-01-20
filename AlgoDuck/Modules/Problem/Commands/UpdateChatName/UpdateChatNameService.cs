using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Commands.UpdateChatName;

public interface IUpdateChatNameService
{
    public Task<Result<UpdateChatNameResult, ErrorObject<string>>> UpdateChatName(UpdateChatNameDto dto,  CancellationToken cancellationToken = default);
}

public class UpdateChatNameService(
    IUpdateChatNameRepository updateChatNameRepository
) :  IUpdateChatNameService
{
    public async Task<Result<UpdateChatNameResult, ErrorObject<string>>> UpdateChatName(UpdateChatNameDto dto, CancellationToken cancellationToken = default)
    {
        return await updateChatNameRepository.UpdateChatName(dto, cancellationToken);    
    }
}