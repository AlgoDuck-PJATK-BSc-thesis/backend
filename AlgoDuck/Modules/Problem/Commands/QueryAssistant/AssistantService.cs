using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AlgoDuck.Models;
using AlgoDuck.Modules.Problem.Queries.GetProblemDetailsByName;
using Microsoft.IdentityModel.Tokens;
using OpenAI;
using OpenAI.Chat;

namespace AlgoDuck.Modules.Problem.Commands.QueryAssistant;

public interface IAssistantService
{
    internal IAsyncEnumerable<ChatCompletionStreamedDto> GetAssistanceAsync(AssistantRequestDto request, CancellationToken cancellationToken = default);
}

public class SimpleStreamingUpdate
{
    public required ChatMessageContent ContentUpdate { get; init; }
}

public class AssistantService(
    OpenAIClient openAiClient,
    IAssistantRepository assistantRepository
    ) : IAssistantService
{
    public async IAsyncEnumerable<ChatCompletionStreamedDto> GetAssistanceAsync(AssistantRequestDto request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatData = await assistantRepository.GetChatDataIfExistsAsync(request, cancellationToken);
        var problemData = await assistantRepository.GetProblemDetailsAsync(request.ExerciseId, cancellationToken: cancellationToken);
        
        var assistantQueryJson = BuildAssistantQuery(request, problemData, chatData);
        var message = ChatMessage.CreateUserMessage(assistantQueryJson);

        var parser = new SignalParser();
        AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = openAiClient
            .GetChatClient("gpt-5-nano")
            .CompleteChatStreamingAsync([message],
                new ChatCompletionOptions(),
                cancellationToken);
        
        var nameBuilder = new StringBuilder();
        List<StringBuilder> textBuilders = [];
        List<StringBuilder> codeBuilders = [];
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
                        codeBuilders.Add(new StringBuilder());
                    }
                    codeBuilders.Last().Append(completionUpdate.Message);
                    break;
                case ContentType.Text:
                    if (currentlyWriting != ContentType.Text)
                    {
                        currentlyWriting = ContentType.Text;
                        textBuilders.Add(new StringBuilder());
                    }
                    textBuilders.Last().Append(completionUpdate.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            yield return completionUpdate;
        }

        var conversationName = nameBuilder.ToString().Trim();
        
        await assistantRepository.CreateNewChatMessage(new ChatMessageInsertDto
        {
            Author = MessageAuthor.User,
            UserId = request.UserId,
            ProblemId = request.ExerciseId,
            ChatName = conversationName,
            TextFragments =
            [
                new ChatMessageTextFragment
                {
                    Message = request.Query.IsNullOrEmpty() ? "User did not provide query" : Encoding.UTF8.GetString(Convert.FromBase64String(request.Query)).Trim(),
                }
            ]
        }, cancellationToken);
        
        
        
        await assistantRepository.CreateNewChatMessage(new ChatMessageInsertDto
        {
            Author = MessageAuthor.Assistant,
            UserId = request.UserId,
            ProblemId = request.ExerciseId,
            ChatName = conversationName,
            CodeFragments = codeBuilders.Where(cb => !cb.ToString().IsNullOrEmpty()).Select(x => new ChatMessageCodeFragment
            {
                Message = x.ToString().Trim()
            }).ToList(),
            TextFragments = textBuilders.Where(tb => !tb.ToString().IsNullOrEmpty()).Select(x => new ChatMessageTextFragment
            {
                Message = x.ToString().Trim()
            }).ToList()
        }, cancellationToken);
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
    

    private static string BuildAssistantQuery(AssistantRequestDto request, ProblemDto problemData, AssistantChat? chatData)
    {
        var duckType = "pirate"; // TODO: Fetch from db once that's ready
        var input = new ModelInputJsonSchema
        {
            ChatName = chatData?.Name,
            ProblemDescription = problemData.Description,
            ProvidedTemplate = problemData.TemplateContents,
            Role = $"{duckType} a helpful programming ducky/ADS field expert",
            Instructions =
                "Help out the user with the exercise. If a chatName is not provided generate it based on user query and exercise contents. Use markdown where possible to emphasise points. Titles do not need to be continuous strings; \"some title\" this is preferred to \"some-title\"",
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
            OutputSchema = "You MUST output all content inside <sig> blocks. No text may appear outside\nof <sig>…</sig>.\n\nAllowed block types:\n  - <sig type=\"name\">CHATNAME</sig>\n  - <sig type=\"text\">NATURAL_TEXT</sig>\n  - <sig type=\"code\">CODE_SNIPPET</sig>\n\nRules:\n1. A block MUST have this exact shape:\n       <sig type=\"TYPE\">CONTENT</sig>\n   - tag name \"sig\" must be lowercase\n   - attribute name must be exactly \"type\"\n   - TYPE must be: name, text, or code\n   - opening tag must be a single uninterrupted piece of text (no newlines)\n\n2. CONTENT rules:\n   - No \"<\" or \">\" inside CONTENT; escape them as &lt; and &gt; if needed. If any '<' or '>' symbols are present in content the application WILL falter\n   - No nested <sig> blocks inside CONTENT.\n   - For code, output the raw code (escaped if containing < or >).\n   - For text, output only natural language or explanation.\n   - For name, output only the chatName string.\n\n3. Every opening tag MUST have a matching </sig> closing tag exactly.\n\n4. If the user provides a chatName in their input JSON:\n   - The FIRST block you emit MUST be:\n         <sig type=\"name\">THE_CHAT_NAME</sig>\n\n5. Never emit anything that resembles a tag unless it is a valid <sig> block.\n\nThese constraints are strict to ensure streaming parsability. Follow them exactly. The application WILL falter if you do not\n"
        };
        return JsonSerializer.Serialize(input);
    }
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
    public required Guid UserId { get; set; }
    public string ChatName { get; set; } = "";
    public ICollection<ChatMessageTextFragment> TextFragments { get; set; } = [];
    public ICollection<ChatMessageCodeFragment> CodeFragments { get; set; } = [];
}

public class ChatMessageTextFragment
{
    public required string Message { get; set; }
}

public class ChatMessageCodeFragment
{
    public required string Message { get; set; }
}