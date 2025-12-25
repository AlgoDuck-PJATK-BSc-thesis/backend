using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace AlgoDuck.Modules.Problem.Commands.UpdateChatName;

public interface IUpdateChatNameRepository
{
    public Task<Result<UpdateChatNameResult, string>> UpdateChatName(UpdateChatNameDto dto,
        CancellationToken cancellationToken = default);
}

public class UpdateChatNameRepository(
    ApplicationCommandDbContext dbContext
) : IUpdateChatNameRepository
{
    public async Task<Result<UpdateChatNameResult, string>> UpdateChatName(UpdateChatNameDto dto,
        CancellationToken cancellationToken = default)
    {
        var chatsUpdated = await dbContext.AssistantChats.Where(e =>
                e.ProblemId == dto.ProblemId && e.UserId == dto.UserId && e.Name == dto.ChatName)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.Name, dto.ChatName), cancellationToken: cancellationToken);
        
        var messagesUpdated = await dbContext.AssistanceMessages
            .Where(e => e.ProblemId == dto.ProblemId && e.UserId == dto.UserId && e.ChatName == dto.ChatName)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.ChatName, dto.ChatName),
                cancellationToken: cancellationToken);

        if (chatsUpdated == 0) return Result<UpdateChatNameResult, string>.Err("Chat not found");
        
        return Result<UpdateChatNameResult, string>.Ok(new UpdateChatNameResult
        {
            NewChatName = dto.ChatName,
            MessagesUpdated = messagesUpdated,
        });
    }
}

