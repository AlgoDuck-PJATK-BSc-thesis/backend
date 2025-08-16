using System.Text;

namespace ExecutorService.Executor.Models;

public class TestCase(
    string testInput,
    string expectedOutput,
    string call,
    string funcName,
    string display,
    string displayRes,
    string isPublic
    )
{
    public string TestInput => testInput;
    public string ExpectedOutput => expectedOutput;
    public string Call => call;
    public string FuncName => funcName;
    public string Display => display;
    public string DisplayRes => displayRes;
    public string IsPublic => isPublic;

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
        var offset = 0;
        
        while (true)
        {
            string testCasesContents;
            try
            {
                testCasesContents = ConsumeTagContents(testCasesString, "tc", ref offset);
            }
            catch (Exception e)
            {
                return testCases;    
            }

            var huh = 0;
            var setup = ConsumeTagContents(testCasesContents, "setup", ref huh);
            var call = ConsumeTagContents(testCasesContents, "call", ref huh);
            var expected = ConsumeTagContents(testCasesContents, "expected", ref huh);
            var funcName = ConsumeTagContents(testCasesContents, "funcName", ref huh);
            var display = ConsumeTagContents(testCasesContents, "display", ref huh);
            var displayRes = ConsumeTagContents(testCasesContents, "displayRes", ref huh);
            var isPublic = ConsumeTagContents(testCasesContents, "public", ref huh);
            testCases.Add(new TestCase(setup, call, expected, funcName, display, displayRes, isPublic));
        }
        
    }
}