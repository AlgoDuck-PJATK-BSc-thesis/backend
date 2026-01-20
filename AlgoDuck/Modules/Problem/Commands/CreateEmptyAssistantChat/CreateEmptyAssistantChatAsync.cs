using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.CreateEmptyAssistantChat;

public interface ICreateEmptyAssistantChatRepository
{
    public Task<Result<AssistantChatCreationDto, ErrorObject<string>>> CreateEmptyAssistantChatAsync(
        EmptyAssistantChatRequest requestDto, CancellationToken cancellationToken = default);
}

public class CreateEmptyAssistantChatRepository : ICreateEmptyAssistantChatRepository
{
    private readonly ApplicationCommandDbContext _dbContext;

    public CreateEmptyAssistantChatRepository(ApplicationCommandDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<AssistantChatCreationDto, ErrorObject<string>>> CreateEmptyAssistantChatAsync(
        EmptyAssistantChatRequest requestDto, CancellationToken cancellationToken = default)
    {
        var existingEmptyChat = await _dbContext.AssistantChats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync( c => c.ProblemId == requestDto.ProblemId && c.UserId == requestDto.UserId && c.Messages.Count == 0, cancellationToken: cancellationToken);

        if (existingEmptyChat != null) return Result<AssistantChatCreationDto, ErrorObject<string>>.Ok(new AssistantChatCreationDto()
        {
            ChatId = existingEmptyChat.Id
        });
        
        var entityEntry = await _dbContext.AssistantChats.AddAsync(new AssistantChat
        {
            ProblemId = requestDto.ProblemId,
            UserId = requestDto.UserId,
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<AssistantChatCreationDto, ErrorObject<string>>.Ok(new AssistantChatCreationDto
        {
            ChatId = entityEntry.Entity.Id,
        });
    }
}
