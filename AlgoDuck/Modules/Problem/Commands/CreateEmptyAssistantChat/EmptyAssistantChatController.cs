using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.CreateEmptyAssistantChat;

[ApiController]
[Authorize]
[Route("api/problem/assistant/chat")]
public class EmptyAssistantChatController : ControllerBase
{
    private readonly ICreateEmptyAssistantChatService _service;

    public EmptyAssistantChatController(ICreateEmptyAssistantChatService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNewChatAsync([FromQuery] Guid problemId, CancellationToken cancellationToken)
    {
        return await User.GetUserId().BindAsync(async userId => await _service.CreateEmptyAssistantChatAsync(
            new EmptyAssistantChatRequest
            {
                ProblemId = problemId,
                UserId = userId
            }, cancellationToken)).ToActionResultAsync();
    }
}

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

public class EmptyAssistantChatRequest
{
    public required Guid UserId { get; set; }
    public required Guid ProblemId { get; set; }
}

public class AssistantChatCreationDto
{
    public required Guid ChatId { get; set; }
}