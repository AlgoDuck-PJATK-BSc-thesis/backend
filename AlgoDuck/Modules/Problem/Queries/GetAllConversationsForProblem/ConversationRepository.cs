using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.Types;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetAllConversationsForProblem;


public interface IConversationRepository
{
    public Task<Result<PageData<AssistanceMessageDto>, ErrorObject<string>>> GetPagedChatData(PageRequestDto pageRequestDto, CancellationToken cancellation = default);
}

public class ConversationRepository(
    ApplicationQueryDbContext dbContext
    ) : IConversationRepository
{
    public async Task<Result<PageData<AssistanceMessageDto>, ErrorObject<string>>> GetPagedChatData(PageRequestDto pageRequestDto, CancellationToken cancellation = default)
    {
        var totalItems = await dbContext.AssistanceMessages.CountAsync(c => c.ChatId == pageRequestDto.ChatId, cancellationToken: cancellation);

        if (await dbContext.AssistantChats.AnyAsync(
                c => c.Id == pageRequestDto.ChatId && c.UserId != pageRequestDto.UserId,
                cancellationToken: cancellation))
        {
            return Result<PageData<AssistanceMessageDto>, ErrorObject<string>>.Err(ErrorObject<string>.Forbidden("Cannot access another user's chat"));
        }
        
        var pageCount = (int) Math.Ceiling((decimal)totalItems / pageRequestDto.PageSize);

        var actualPage = Math.Max(1, Math.Min(pageRequestDto.Page, pageCount));
        var chat = await dbContext.AssistantChats
                .Where(c => c.Id == pageRequestDto.ChatId && c.UserId == pageRequestDto.UserId)
                .Select(c =>
                    new ChatDto
                    {
                        ChatName = c.Name,
                        Messages = c.Messages
                            .OrderByDescending(m => m.CreatedOn)
                            .Skip((actualPage - 1) * pageRequestDto.PageSize)
                            .Take(pageRequestDto.PageSize)
                            .Select(m => new AssistanceMessageDto
                            {
                                Fragments = m.Fragments.Select(f => new MessageFragmentDto
                                {
                                    Content = f.Content,
                                    Type = f.FragmentType,
                                }).ToList(),
                                MessageAuthor = m.IsUserMessage ? MessageAuthor.User : MessageAuthor.Assistant,
                                CreatedOn = m.CreatedOn
                            }).ToList()
                    }).FirstOrDefaultAsync(cancellationToken: cancellation);
        
        if (chat == null)
        {
            return Result<PageData<AssistanceMessageDto>, ErrorObject<string>>.Ok(new PageData<AssistanceMessageDto>
            {
                CurrPage = 1,
                PageSize = pageRequestDto.PageSize,
                NextCursor = 2,
                TotalItems = 0,
                Items = [],
            });
        }
        
        return Result<PageData<AssistanceMessageDto>, ErrorObject<string>>.Ok(new PageData<AssistanceMessageDto>
        {
            CurrPage = actualPage,
            PrevCursor = actualPage > 1 ? actualPage - 1 : null,
            NextCursor = actualPage < pageCount ? actualPage + 1 : null,
            PageSize = pageRequestDto.PageSize,
            TotalItems = totalItems,
            Items = chat.Messages
        });
    }
}