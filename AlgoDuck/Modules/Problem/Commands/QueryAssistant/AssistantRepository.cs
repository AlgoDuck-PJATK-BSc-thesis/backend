using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AlgoDuck.Modules.Problem.Commands.QueryAssistant;

public interface IAssistantRepository
{
    public Task<Result<AssistantChat?, ErrorObject<string>>> GetChatDataIfExistsAsync(AssistantRequestDto request,
        CancellationToken cancellationToken = default);

    public Task<Result<int, ErrorObject<string>>> CreateNewChatMessagesAsync(
        CancellationToken cancellationToken = default,
        params ChatMessageInsertDto[] request); /* I hate the fact that cancellation token is not last in the arg list */
}

public class AssistantRepository : IAssistantRepository
{
    private readonly ApplicationCommandDbContext _dbContext;

    public AssistantRepository(ApplicationCommandDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<AssistantChat?, ErrorObject<string>>> GetChatDataIfExistsAsync(AssistantRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.AssistantChats
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
        const int maxFragmentLength = 2048;

        var chatMessagesGrouped = request.GroupBy(e => e.ChatId).ToList();
        var messageCounter = 0;
        foreach (var chatMessageInsertDto in chatMessagesGrouped)
        {
            var assistantChat = await _dbContext.AssistantChats.FirstOrDefaultAsync(
                ch => ch.Id == chatMessageInsertDto.Key,
                cancellationToken: cancellationToken);

            if (assistantChat == null)
            {
                var firstMessage = chatMessageInsertDto.ToList().First();
                assistantChat = new AssistantChat
                {
                    Id = firstMessage.ChatId,
                    Name = firstMessage.ChatName.IsNullOrEmpty() ? firstMessage.ChatName : "Unnamed",
                    ProblemId = firstMessage.ProblemId,
                    UserId = firstMessage.UserId,
                };
                _dbContext.AssistantChats.Add(assistantChat);
            }

            if (assistantChat.Name.IsNullOrEmpty() && chatMessageInsertDto.Any())
            {
                assistantChat.Name = chatMessageInsertDto.ToList()[0].ChatName;
            }

            foreach (var chatMessage in chatMessageInsertDto.ToList())
            {
                messageCounter++;
                var fragments = new List<AssistantMessageFragment>();

                foreach (var chf in chatMessage.ChatFragments)
                {
                    var content = chf.MessageContents.ToString();

                    if (content.Length <= maxFragmentLength)
                    {
                        fragments.Add(new AssistantMessageFragment
                        {
                            Content = content,
                            FragmentType = chf.FragmentType
                        });
                    }
                    else
                    {
                        var chunks = SplitIntoChunks(content, maxFragmentLength);
                        fragments.AddRange(chunks.Select(chunk => new AssistantMessageFragment
                        {
                            Content = chunk,
                            FragmentType = chf.FragmentType
                        }));
                    }
                }

                assistantChat.Messages.Add(new AssistanceMessage
                {
                    Fragments = fragments,
                    IsUserMessage = chatMessage.Author == MessageAuthor.User
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<int, ErrorObject<string>>.Ok(messageCounter);
    }

    private static IEnumerable<string> SplitIntoChunks(string content, int chunkSize)
    {
        for (var i = 0; i < content.Length; i += chunkSize)
        {
            yield return content.Substring(i, Math.Min(chunkSize, content.Length - i));
        }
    }
}