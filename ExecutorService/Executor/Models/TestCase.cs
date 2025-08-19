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
        var closingTag = $"</{tag}>";
        
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

    private static string SanitizeTestCaseFragment(string toSanitize, string entrypointClassName)
    {
        return toSanitize.Replace("${ENTRYPOINT_CLASS_NAME}", entrypointClassName);
    }
    
    public static List<TestCase> ParseTestCases(string testCasesString, string entrypointClassName)
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
            var setup = SanitizeTestCaseFragment(ConsumeTagContents(testCasesContents, "setup", ref huh), entrypointClassName);
            var call = SanitizeTestCaseFragment(ConsumeTagContents(testCasesContents, "call", ref huh), entrypointClassName);
            var expected = SanitizeTestCaseFragment(ConsumeTagContents(testCasesContents, "expected", ref huh), entrypointClassName);
            var funcName = SanitizeTestCaseFragment(ConsumeTagContents(testCasesContents, "funcName", ref huh), entrypointClassName);
            var display = SanitizeTestCaseFragment(ConsumeTagContents(testCasesContents, "display", ref huh), entrypointClassName);
            var displayRes = SanitizeTestCaseFragment(ConsumeTagContents(testCasesContents, "displayRes", ref huh), entrypointClassName);
            var isPublic = SanitizeTestCaseFragment(ConsumeTagContents(testCasesContents, "public", ref huh), entrypointClassName);
            testCases.Add(new TestCase(setup, expected, call, funcName, display, displayRes, isPublic));
        }
        
    }
}