using System.Text.Json.Serialization;
using AlgoDuck.Models;

namespace AlgoDuck.Modules.Problem.Commands.QueryAssistant;


public class ModelInputJsonSchema
{
    public required string Role { get; set; } 
    public required string ProblemDescription { get; set; }    
    public required string ProvidedTemplate { get; set; }
    public required string UserCode { get; set; }
    public required string UserQueryToAssistant { get; set; }
    public string? ChatName { get; set; }
    public IEnumerable<TestCaseData> PublicTestCases { get; set; } = [];
    public IEnumerable<AssistantChatMessage> MessagesInChat10Newest { get; set; } = [];
    public required string Instructions { get; set; }
    public required string Restrictions { get; set; }
    public required string OutputSchema { get; set; }
}

public class TestCaseData
{
    public required string TestCaseInput { get; set; }
    public required string TestCaseExpectedOutput { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageAuthor
{
    Assistant, User
}

public class AssistantChatMessage
{
    public List<MessageFragmentDto> MessageFragments { get; set; } = [];
    public MessageAuthor Author { get; set; } = MessageAuthor.User;
}

public class MessageFragmentDto
{
    public required string FragmentContent { get; set; }
    public required FragmentType Type { get; set; }
}