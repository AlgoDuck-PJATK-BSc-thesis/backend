using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Commands.CreateEmptyAssistantChat;

public interface ICreateEmptyAssistantChatService
{
    public Task<Result<AssistantChatCreationDto, ErrorObject<string>>> CreateEmptyAssistantChatAsync(
        EmptyAssistantChatRequest requestDto, CancellationToken cancellationToken = default);
}

public class CreateEmptyAssistantChatService : ICreateEmptyAssistantChatService
{
    private readonly ICreateEmptyAssistantChatRepository _repository;

    public CreateEmptyAssistantChatService(ICreateEmptyAssistantChatRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AssistantChatCreationDto, ErrorObject<string>>> CreateEmptyAssistantChatAsync(EmptyAssistantChatRequest requestDto, CancellationToken cancellationToken = default)
    {
        return await  _repository.CreateEmptyAssistantChatAsync(requestDto, cancellationToken); 
    }
}