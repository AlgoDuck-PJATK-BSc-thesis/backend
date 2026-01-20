using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Commands.UpdateChatName;

public interface IUpdateChatNameService
{
    public Task<Result<UpdateChatNameResult, ErrorObject<string>>> UpdateChatName(UpdateChatNameDto dto,  CancellationToken cancellationToken = default);
}

public class UpdateChatNameService :  IUpdateChatNameService
{
    private readonly IUpdateChatNameRepository _updateChatNameRepository;

    public UpdateChatNameService(IUpdateChatNameRepository updateChatNameRepository)
    {
        _updateChatNameRepository = updateChatNameRepository;
    }

    public async Task<Result<UpdateChatNameResult, ErrorObject<string>>> UpdateChatName(UpdateChatNameDto dto, CancellationToken cancellationToken = default)
    {
        return await _updateChatNameRepository.UpdateChatName(dto, cancellationToken);    
    }
}