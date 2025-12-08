using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AlgoDuck.DAL;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Extensions;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetAllConversationsForProblem;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ChatDataController(
    IConversationService conversationService
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPagedChatDataAsync([FromQuery] int page, [FromQuery] int pageSize,
        [FromQuery] [MaxLength(128)] string chatName, [FromQuery] Guid problemId, CancellationToken cancellationToken)
    {
        return Ok(new StandardApiResponse<PageData<AssistanceMessageDto>>
        {
            Body = await conversationService.GetPagedChatData(new PageRequestDto
            {
                ChatName = chatName,
                PageSize = pageSize,
                Page = page,
                ProblemId = problemId,
                UserId = User.GetUserId()
            }, cancellationToken)
        });
    }
}

public interface IConversationService
{
    public Task<PageData<AssistanceMessageDto>> GetPagedChatData(PageRequestDto pageRequestDto, CancellationToken cancellation = default);
}

public class ConversationService(
    IConversationRepository conversationRepository
    ) : IConversationService
{
    public async Task<PageData<AssistanceMessageDto>> GetPagedChatData(PageRequestDto pageRequestDto, CancellationToken cancellation = default)
    {
        return await conversationRepository.GetPagedChatData(pageRequestDto, cancellation);
    }
}

public interface IConversationRepository
{
    public Task<PageData<AssistanceMessageDto>> GetPagedChatData(PageRequestDto pageRequestDto, CancellationToken cancellation = default);
}

public class ConversationRepository(
    ApplicationQueryDbContext dbContext
    ) : IConversationRepository
{
    public async Task<PageData<AssistanceMessageDto>> GetPagedChatData(PageRequestDto pageRequestDto, CancellationToken cancellation = default)
    {
        var chat = await dbContext.AssistantChats
                .Where(c => c.ProblemId == pageRequestDto.ProblemId
                            && c.UserId == pageRequestDto.UserId
                            && c.Name == pageRequestDto.ChatName)
                .Select(c =>
                    new ChatDto
                    {
                        ChatName = c.Name,
                        Messages = c.Messages
                            .OrderByDescending(m => m.CreatedOn)
                            .Skip((pageRequestDto.Page - 1) * pageRequestDto.PageSize)
                            .Take(pageRequestDto.PageSize)
                            .Select(m => new AssistanceMessageDto()
                            {
                                Message = m.Content,
                                MessageAuthor = m.IsUserMessage ? MessageAuthor.User : MessageAuthor.Assistant
                            }).ToList()
                    }).FirstOrDefaultAsync(cancellationToken: cancellation);
        
        if (chat == null) 
            throw new NotFoundException(pageRequestDto.ChatName);
        
        return new PageData<AssistanceMessageDto>
        {
            CurrPage = pageRequestDto.Page,
            PageSize = pageRequestDto.PageSize,
            TotalItems = await dbContext.AssistanceMessages.CountAsync(c => c.ProblemId == pageRequestDto.ProblemId && c.UserId == pageRequestDto.UserId && c.ChatName == pageRequestDto.ChatName, cancellationToken: cancellation),
            Items = chat.Messages
        };
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageAuthor
{
    Assistant,
    User
}

public class PageRequestDto
{
    
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required string ChatName { get; set; }
    public required Guid ProblemId { get; set; }
    public required Guid UserId { get; set; }
}

public class ChatDto
{
    public required string ChatName { get; set; }    
    public required ICollection<AssistanceMessageDto> Messages { get; set; }
}

public class AssistanceMessageDto
{
    public required string Message { get; set; }
    public required MessageAuthor MessageAuthor { get; set; }
}