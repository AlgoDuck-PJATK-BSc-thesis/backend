using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.UpdateChatName;

public interface IUpdateChatNameRepository
{
    public Task<Result<UpdateChatNameResult, ErrorObject<string>>> UpdateChatName(UpdateChatNameDto dto,
        CancellationToken cancellationToken = default);
}

public class UpdateChatNameRepository : IUpdateChatNameRepository
{
    private readonly ApplicationCommandDbContext _dbContext;

    public UpdateChatNameRepository(ApplicationCommandDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UpdateChatNameResult, ErrorObject<string>>> UpdateChatName(UpdateChatNameDto dto,
        CancellationToken cancellationToken = default)
    {
        if (await _dbContext.AssistantChats.AsNoTracking().AnyAsync(e => e.Id == dto.ChatId && e.UserId != dto.UserId, cancellationToken: cancellationToken))
        {
            return Result<UpdateChatNameResult, ErrorObject<string>>.Err(
                ErrorObject<string>.Forbidden("Cannot rename chat that you didn't make"));
        }
        
        var chatsUpdated = await _dbContext.AssistantChats.Where(e => e.Id == dto.ChatId && e.UserId == dto.UserId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.Name, dto.NewChatName), cancellationToken: cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (chatsUpdated == 0) return Result<UpdateChatNameResult, ErrorObject<string>>.Err(ErrorObject<string>.NotFound("Chat not found"));
        
        return Result<UpdateChatNameResult, ErrorObject<string>>.Ok(new UpdateChatNameResult
        {
            NewChatName = dto.NewChatName,
        });
    }
}