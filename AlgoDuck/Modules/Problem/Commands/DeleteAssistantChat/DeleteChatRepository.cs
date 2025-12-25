using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.DeleteAssistantChat;

public interface IDeleteChatRepository
{
    public Task<Result<DeleteChatDtoResult, ErrorObject<string>>> Delete(DeleteChatDto dto,
        CancellationToken cancellationToken = default);
}

public class DeleteChatRepository(
    ApplicationCommandDbContext dbContext
) : IDeleteChatRepository
{
    public async Task<Result<DeleteChatDtoResult, ErrorObject<string>>> Delete(DeleteChatDto dto,
        CancellationToken cancellationToken = default)
    {
        var chatsDeleted = await dbContext.AssistantChats.Where(e =>
                e.ProblemId == dto.ProblemId && e.UserId == dto.UserId && e.Name == dto.ChatName)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);

        var messagesDeleted = await dbContext.AssistanceMessages
            .Where(e => e.ProblemId == dto.ProblemId && e.UserId == dto.UserId && e.ChatName == dto.ChatName)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);

        if (chatsDeleted == 0)
            return Result<DeleteChatDtoResult, ErrorObject<string>>.Err(ErrorObject<string>.NotFound("Chat not found"));

        return Result<DeleteChatDtoResult, ErrorObject<string>>.Ok(new DeleteChatDtoResult
        {
            MessagesDeleted = messagesDeleted,
        });
    }
}