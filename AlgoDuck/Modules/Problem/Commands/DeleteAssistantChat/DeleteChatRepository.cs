using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.DeleteAssistantChat;

public interface IDeleteChatRepository
{
    public Task<Result<DeleteChatDtoResult, ErrorObject<string>>> Delete(DeleteChatDto dto,
        CancellationToken cancellationToken = default);
}

public class DeleteChatRepository : IDeleteChatRepository
{
    private readonly ApplicationCommandDbContext _dbContext;

    public DeleteChatRepository(ApplicationCommandDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DeleteChatDtoResult, ErrorObject<string>>> Delete(DeleteChatDto dto,
        CancellationToken cancellationToken = default)
    {
        var chatsDeleted = await _dbContext.AssistantChats.Where(e => e.Id == dto.ChatId)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);

        var messagesDeleted = await _dbContext.AssistanceMessages
            .Where(e => e.ChatId == dto.ChatId)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);

        if (chatsDeleted == 0)
            return Result<DeleteChatDtoResult, ErrorObject<string>>.Err(ErrorObject<string>.NotFound("Chat not found"));

        return Result<DeleteChatDtoResult, ErrorObject<string>>.Ok(new DeleteChatDtoResult
        {
            MessagesDeleted = messagesDeleted,
        });
    }
}