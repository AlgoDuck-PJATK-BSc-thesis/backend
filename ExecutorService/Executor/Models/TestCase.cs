using System.Text;

namespace ExecutorService.Executor.Models;

public class TestCase(string testInput, string expectedOutput, string call, string funcName)
{
    public string TestInput => testInput;
    public string ExpectedOutput => expectedOutput;
    public string Call => call;
    public string FuncName => funcName;

    private static string ConsumeTagContents(string contents, string tag, ref int offset)
    {
        var openingTag = $"<{tag}>";
        var closingTag = $"<{tag}/>";
        
        var openingTagIndex = contents.IndexOf(openingTag, offset, StringComparison.Ordinal);
        if (openingTagIndex == -1)
        {
            throw new Exception($"Opening tag <{tag}> not found");
        }
        
        offset = openingTagIndex + openingTag.Length;
        
        var closingTagIndex = contents.IndexOf(closingTag, offset, StringComparison.Ordinal);
        if (closingTagIndex == -1)
        {
            throw new Exception($"Closing tag <{tag}/> not found");
        }
        
        var content = contents.Substring(offset, closingTagIndex - offset);
        
        offset = closingTagIndex + closingTag.Length;
        
        return content.Trim();
    }
    
    public static List<TestCase> ParseTestCases(string testCasesString)
    {
        List<TestCase> testCases = [];
        int offset = 0;
        
        while (true)
        {
            string testCasesContents;
            try
            {
                testCasesContents = ConsumeTagContents(testCasesString, "setup", ref offset);
            }
            catch (Exception)
            {
                return testCases;    
            }
            var call = ConsumeTagContents(testCasesString, "call", ref offset);
            var expected = ConsumeTagContents(testCasesString, "expected", ref offset);
            testCases.Add(new TestCase(testCasesContents, expected, call, "sortIntArr"));
        }
        
    }
}