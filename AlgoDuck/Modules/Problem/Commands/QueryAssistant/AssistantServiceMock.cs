using System.Runtime.CompilerServices;
using OpenAI.Chat;

namespace AlgoDuck.Modules.Problem.Commands.QueryAssistant;

public class AssistantServiceMock(
    IAssistantRepository assistantRepository
) : IAssistantService
{
    
    private const string ModelOutputMock =
        "<sig type=\"name\">Pirate Ducky Two Sum Helper</sig>\n<sig type=\"text\">Goal: find two indices i and j (i != j) such that nums[i] + nums[j] == target. Use a single pass with a hash map to get O(n) time and O(n) space.\n\nIdea:\n- Iterate the array once.\n- For each nums[i], </ type/><</>.compute complement = target - nums[i].\n- If complement is already in the map, return indices [map.get(complement), i].\n- Otherwise store nums[i] -> i in the map and continue.\nThis guarantees you never use the same element twice because you only match with previously seen elements.</sig>\n<sig type=\"code\">public class Main {\n public static int[] twoSum(int[] nums, int target) {\n java.util.Map<Integer, Integer> seen = new java.util.HashMap<>();\n for (int i = 0; i < nums.length; i++) {\n int complement = target - nums[i];\n if (seen.containsKey(complement)) {\n return new int[] { seen.get(complement), i };\n }\n seen.put(nums[i], i);\n }\n throw new IllegalArgumentException(\"No two sum solution\");\n }\n}</sig>";
    
    private static async IAsyncEnumerable<SimpleStreamingUpdate> EnumerateInput(string input, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var filePos = 0;
        Random rand = new();
        await Task.Delay(rand.Next(4000, 7500), ct);
        while (filePos < input.Length)
        {
            var charsToConsume = rand.Next(1, 10);
            var chars = input.Substring(filePos, Math.Min(charsToConsume, input.Length - filePos));
            filePos += charsToConsume;
            yield return new SimpleStreamingUpdate
            {
                ContentUpdate = new ChatMessageContent(chars)
            };
            await Task.Delay(rand.Next(50, 150), ct);
        }
    }
    
    public async IAsyncEnumerable<ChatCompletionStreamedDto> GetAssistanceAsync(AssistantRequestDto request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var parser = new SignalParser();
        await foreach (var output in parser.Parse(EnumerateInput(ModelOutputMock, cancellationToken),
                           cancellationToken))
        {
            yield return output;
        }
    }
}
