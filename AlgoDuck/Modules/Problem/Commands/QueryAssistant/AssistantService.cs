using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AlgoDuck.Models;
using AlgoDuck.Modules.Item.Queries.GetMyIconItem;
using AlgoDuck.Modules.Problem.Queries.GetProblemDetailsByName;
using AlgoDuck.Modules.Problem.Shared.Repositories;
using AlgoDuck.Shared.Http;
using Microsoft.IdentityModel.Tokens;
using OpenAI;
using OpenAI.Chat;

namespace AlgoDuck.Modules.Problem.Commands.QueryAssistant;

public interface IAssistantService
{
    internal IAsyncEnumerable<Result<ChatCompletionStreamedDto, ErrorObject<string>>> GetAssistanceAsync(AssistantRequestDto request, CancellationToken cancellationToken = default);
}

public class SimpleStreamingUpdate
{
    public required ChatMessageContent ContentUpdate { get; init; }
}

public class AssistantService : IAssistantService
{
    private readonly OpenAIClient _openAiClient;
    private readonly IAssistantRepository _assistantRepository;
    private readonly ISharedProblemRepository _problemRepository;
    private readonly IGetMySelectedIconRepository _iconRepository;

    public AssistantService(ISharedProblemRepository problemRepository, IAssistantRepository assistantRepository, OpenAIClient openAiClient, IGetMySelectedIconRepository iconRepository)
    {
        _problemRepository = problemRepository;
        _assistantRepository = assistantRepository;
        _openAiClient = openAiClient;
        _iconRepository = iconRepository;
    }


    public async IAsyncEnumerable<Result<ChatCompletionStreamedDto, ErrorObject<string>>> GetAssistanceAsync(AssistantRequestDto request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatDataResult = await _assistantRepository.GetChatDataIfExistsAsync(request, cancellationToken);
        if (chatDataResult.IsErr)
        {
            yield return Result<ChatCompletionStreamedDto, ErrorObject<string>>.Err(chatDataResult.AsT1);
            yield break;
        }
        
        var chatData = chatDataResult.AsOk!;

        var problemDataResult = await _problemRepository.GetProblemDetailsAsync(request.ExerciseId, cancellationToken: cancellationToken);
        
        if (problemDataResult.IsErr)
        {
            yield return Result<ChatCompletionStreamedDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Failed getting assistant response. Problem: {request.ExerciseId} not found"));
            yield break;
        }

        var duckItemNameResult = await _iconRepository.GetMySelectedIconAsync(request.UserId, cancellationToken);
        var duckItemName = duckItemNameResult.IsErr ? "default" : duckItemNameResult.AsOk!.ItemName;
        
        var assistantQueryJson = BuildAssistantQuery(request, problemDataResult.AsT0, chatData, duckItemName);
        var message = ChatMessage.CreateUserMessage(assistantQueryJson);

        var parser = new SignalParser();
        AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = _openAiClient
            .GetChatClient("gpt-5-nano")
            .CompleteChatStreamingAsync([message],
                new ChatCompletionOptions(),
                cancellationToken);
        
        var nameBuilder = new StringBuilder();
        List<ChatFragmentBuilder> fragmentList = [];
        
        ContentType? currentlyWriting = null;
        await foreach (var completionUpdate in parser.Parse(TransformOpenAiStream(completionUpdates, cancellationToken), cancellationToken))
        {
            switch (completionUpdate.Type)
            {
                case ContentType.Name:
                    nameBuilder.Append(completionUpdate.Message);
                    break;
                case ContentType.Code:
                    if (currentlyWriting != ContentType.Code)
                    {
                        currentlyWriting = ContentType.Code;
                        fragmentList.Add(new ChatFragmentBuilder
                        {
                            MessageContents = new StringBuilder(),
                            FragmentType = FragmentType.Code
                        });
                    }
                    fragmentList.Last().MessageContents.Append(completionUpdate.Message);
                    break;
                case ContentType.Text:
                    if (currentlyWriting != ContentType.Text)
                    {
                        currentlyWriting = ContentType.Text;
                        fragmentList.Add(new ChatFragmentBuilder
                        {
                            MessageContents = new StringBuilder(),
                            FragmentType = FragmentType.Text
                        });
                    }
                    fragmentList.Last().MessageContents.Append(completionUpdate.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            yield return Result<ChatCompletionStreamedDto, ErrorObject<string>>.Ok(completionUpdate);
        }

        var generatedChatName = nameBuilder.ToString().Trim();

        await _assistantRepository.CreateNewChatMessagesAsync(cancellationToken, new ChatMessageInsertDto
        {
            Author = MessageAuthor.User,
            UserId = request.UserId,
            ProblemId = request.ExerciseId,
            ChatId = request.ChatId,
            ChatName = generatedChatName,
            ChatFragments = [new ChatFragmentBuilder
            {
                MessageContents = new StringBuilder(request.Query),
                FragmentType = FragmentType.Text,
            }],
        }, new ChatMessageInsertDto
        {
            Author = MessageAuthor.Assistant,
            UserId = request.UserId,
            ChatId = request.ChatId,
            ProblemId = request.ExerciseId,
            ChatName = generatedChatName,
            ChatFragments = fragmentList,
        });
        
    }

    private static async IAsyncEnumerable<SimpleStreamingUpdate> TransformOpenAiStream(IAsyncEnumerable<StreamingChatCompletionUpdate> chatData, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {

        await foreach (var chatUpdate in chatData.WithCancellation(cancellationToken))
        {
            if (chatUpdate.ContentUpdate is null || !(chatUpdate.ContentUpdate.Count > 0))
            {
                continue;
            }
            yield return new SimpleStreamingUpdate
            {
                ContentUpdate = chatUpdate.ContentUpdate
            };
        }
    }
    

    private static string BuildAssistantQuery(AssistantRequestDto request, ProblemDto problemData, AssistantChat? chatData, string itemName)
    {
        var input = new ModelInputJsonSchema
        {
            ChatName = chatData?.Name,
            ProblemDescription = problemData.Description,
            ProvidedTemplate = problemData.TemplateContents,
            Role = $"{itemName} - a helpful programming ducky/ADS field expert",
            Instructions = "Help out the user with the exercise. generate a chat name based on the user query and exercise contents. Use markdown where possible to emphasise points. Titles do not need to be continuous strings; \"some title\" this is preferred to \"some-title\"",
            UserCode = Encoding.UTF8.GetString(Convert.FromBase64String(request.CodeB64)),
            UserQueryToAssistant = Encoding.UTF8.GetString(Convert.FromBase64String(request.Query)),
            PublicTestCases = problemData.TestCases.Select(t => new TestCaseData
            {
                TestCaseExpectedOutput = t.DisplayRes,
                TestCaseInput = t.Display,
            }),
            MessagesInChat10Newest = (chatData?.Messages ?? []).Select(m => new AssistantChatMessage
            {
                MessageFragments = m.Fragments.Select(f => new MessageFragmentDto
                {
                    FragmentContent = f.Content,
                    Type = f.FragmentType
                }).ToList(),
                Author = m.IsUserMessage ? MessageAuthor.User : MessageAuthor.Assistant
            }),
            Restrictions = "Title can be at most 128 characters. Keep code provided to the user at a minimum. Focus on explaining concepts, not a ready to wear solution.",
            OutputSchema = "You MUST output all content inside <sig> blocks. No text may appear outside\nof <sig>…</sig>.\n\nAllowed block types:\n  - <sig type=\"name\">CHATNAME</sig>\n  - <sig type=\"text\">NATURAL_TEXT</sig>\n  - <sig type=\"code\">CODE_SNIPPET</sig>\n\nRules:\n1. A block MUST have this exact shape:\n       <sig type=\"TYPE\">CONTENT</sig>\n   - tag name \"sig\" must be lowercase\n   - attribute name must be exactly \"type\"\n   - TYPE must be: name, text, or code\n   - opening tag must be a single uninterrupted piece of text (no newlines)\n\n2. CONTENT rules:\n   - No nested <sig> blocks inside CONTENT.\n   - For code, output the raw code. You may emit '<' '>' characters so long as they don't resemble a sig block (if they do you CATEGORICALLY MUST escape them with &gt; and &lt;). Use whitespace normally, client side formatting breaks if you don't.\n   - For text, use markdown to emphasise points. Use whitespace normally, client side formatting breaks if you don't.\n   - For name, output only the chatName string.\n\n3. Every opening tag MUST have a matching </sig> closing tag exactly.\n\n4. If the user provides a chatName in their input JSON:\n   - The FIRST block you emit MUST be:\n         <sig type=\"name\">THE_CHAT_NAME</sig>\n\n5. Never emit anything that resembles a tag unless it is a valid <sig> block.\n\nThese constraints are strict to ensure streaming parsability. Follow them exactly. The application WILL falter if you do not\n"
        };
        return JsonSerializer.Serialize(input);
    }
}

public class ChatFragmentBuilder
{
    public required StringBuilder MessageContents { get; set; }
    public required FragmentType FragmentType { get; set; }
}

public class ChatCompletionStreamedDto
{
    public required ContentType Type { get; set; }
    public required string Message { get; set; }
}


public class ChatMessageInsertDto
{
    public required MessageAuthor Author { get; set; }
    public required Guid ProblemId { get; set; }
    public required Guid ChatId { get; set; }
    public required Guid UserId { get; set; }
    public string ChatName { get; set; } = "";
    public ICollection<ChatFragmentBuilder> ChatFragments { get; set; } = [];
}

public class ChatMessageTextFragment
{
    public required string Message { get; set; }
}

public class ChatMessageCodeFragment
{
    public required string Message { get; set; }
}