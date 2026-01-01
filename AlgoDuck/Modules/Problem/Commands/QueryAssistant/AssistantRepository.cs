using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Problem.Queries.GetProblemDetailsByName;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using AlgoDuck.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;

namespace AlgoDuck.Modules.Problem.Commands.QueryAssistant;

public interface IAssistantRepository
{
    public Task<Result<AssistantChat, ErrorObject<string>>> GetChatDataIfExistsAsync(AssistantRequestDto request,
        CancellationToken cancellationToken = default);

    public Task<Result<AssistanceMessage, ErrorObject<string>>> CreateNewChatMessage(ChatMessageInsertDto chatMessage,
        CancellationToken cancellationToken = default);
}

public class AssistantRepository(
    ApplicationCommandDbContext dbContext
) : IAssistantRepository
{
    public async Task<Result<AssistantChat, ErrorObject<string>>> GetChatDataIfExistsAsync(AssistantRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await dbContext.AssistantChats
            .Include(e => e.Messages.OrderByDescending(ie => ie.CreatedOn).Take(10))
            .ThenInclude(e => e.Fragments)
            .FirstOrDefaultAsync(e => e.Id == request.ChatId, cancellationToken);

        return result == null
            ? Result<AssistantChat, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"{request.ChatId}"))
            : Result<AssistantChat, ErrorObject<string>>.Ok(result);
    }


    public async Task<Result<AssistanceMessage, ErrorObject<string>>> CreateNewChatMessage(
        ChatMessageInsertDto chatMessage,
        CancellationToken cancellationToken = default)
    {
        var chat = await dbContext.AssistantChats
            .Where(m => m.ProblemId == chatMessage.ProblemId && m.UserId == chatMessage.UserId &&
                        m.Name == chatMessage.ChatName)
            .FirstOrDefaultAsync(cancellationToken);

        if (chat == null)
        {
            chat = new AssistantChat
            {
                Name = chatMessage.ChatName,
                ProblemId = chatMessage.ProblemId,
                UserId = chatMessage.UserId,
            };
            dbContext.AssistantChats.Add(chat);
        }

        var fragments = chatMessage.TextFragments.Select(f => new AssistantMessageFragment
        {
            Content = f.Message,
            FragmentType = FragmentType.Text,
        }).ToList();

        fragments.AddRange(chatMessage.CodeFragments.Select(f => new AssistantMessageFragment
        {
            Content = f.Message,
            FragmentType = FragmentType.Code,
        }).ToList());

        var newMessage = new AssistanceMessage
        {
            Fragments = fragments,
            IsUserMessage = chatMessage.Author == MessageAuthor.User,
        };
        chat.Messages.Add(newMessage);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<AssistanceMessage, ErrorObject<string>>.Ok(newMessage);
    }
}