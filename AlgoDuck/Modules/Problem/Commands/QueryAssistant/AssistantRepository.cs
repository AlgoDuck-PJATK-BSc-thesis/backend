using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Problem.Queries.GetProblemDetailsByName;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using AlgoDuck.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenAI.Chat;

namespace AlgoDuck.Modules.Problem.Commands.QueryAssistant;

public interface IAssistantRepository
{
    public Task<Result<AssistantChat?, ErrorObject<string>>> GetChatDataIfExistsAsync(AssistantRequestDto request,
        CancellationToken cancellationToken = default);

    public Task<Result<int, ErrorObject<string>>> CreateNewChatMessagesAsync(
        CancellationToken cancellationToken = default,
        params ChatMessageInsertDto[] request); /* I hate the fact that cancellation token is not last in the arg list */
}

public class AssistantRepository(
    ApplicationCommandDbContext dbContext
) : IAssistantRepository
{
    public async Task<Result<AssistantChat?, ErrorObject<string>>> GetChatDataIfExistsAsync(AssistantRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await dbContext.AssistantChats
            .Include(e => e.Messages.OrderByDescending(ie => ie.CreatedOn).Take(10))
            .ThenInclude(e => e.Fragments)
            .FirstOrDefaultAsync(e => e.Id == request.ChatId, cancellationToken);

        return result == null
            ? Result<AssistantChat?, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"{request.ChatId}"))
            : Result<AssistantChat?, ErrorObject<string>>.Ok(result);
    }

    public async Task<Result<int, ErrorObject<string>>> CreateNewChatMessagesAsync(
        CancellationToken cancellationToken = default, params ChatMessageInsertDto[] request)
    {
        var chatMessagesGrouped = request.GroupBy(e => e.ChatId).ToList();
        var messageCounter = 0;
        foreach (var chatMessageInsertDto in chatMessagesGrouped)
        {
            var assistantChat = await dbContext.AssistantChats.FirstOrDefaultAsync(
                ch => ch.Id == chatMessageInsertDto.Key,
                cancellationToken: cancellationToken);

            if (assistantChat == null)
            {
                var firstMessage = chatMessageInsertDto.ToList().First(); 
                assistantChat = new AssistantChat
                {
                    Id = firstMessage.ChatId,
                    Name = firstMessage.ChatName,
                    ProblemId = firstMessage.ProblemId,
                    UserId = firstMessage.UserId,
                };
                dbContext.AssistantChats.Add(assistantChat);
            }

            if (assistantChat.Name.IsNullOrEmpty() && chatMessageInsertDto.Any())
            {
                assistantChat.Name = chatMessageInsertDto.ToList()[0].ChatName;
            }
            foreach (var chatMessage in chatMessageInsertDto.ToList())
            {
                messageCounter++;
                assistantChat.Messages.Add(new AssistanceMessage
                {
                    Fragments = chatMessage.ChatFragments.Select(chf => new AssistantMessageFragment
                    {
                        Content = chf.MessageContents.ToString(),
                        FragmentType = chf.FragmentType
                    }).ToList(),
                    IsUserMessage = chatMessage.Author == MessageAuthor.User
                });
            }
        }
        
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<int, ErrorObject<string>>.Ok(messageCounter);
    }
}