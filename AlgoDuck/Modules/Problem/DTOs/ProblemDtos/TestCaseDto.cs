namespace AlgoDuck.Modules.Problem.DTOs.ProblemDtos;

public class TestCaseDto
{
    public string TestCaseId { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public string DisplayRes { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false;

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
    
    public static List<TestCaseDto> ParseTestCases(string testCasesString, string entrypointClassName)
    {
        List<TestCaseDto> testCases = [];
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
            var testCaseId = SanitizeTestCaseFragment(ConsumeTagContents(testCasesContents, "id", ref huh), entrypointClassName);
            SanitizeTestCaseFragment(ConsumeTagContents(testCasesContents, "setup", ref huh), entrypointClassName);
            SanitizeTestCaseFragment(ConsumeTagContents(testCasesContents, "call", ref huh), entrypointClassName);
            SanitizeTestCaseFragment(ConsumeTagContents(testCasesContents, "expected", ref huh), entrypointClassName);
            SanitizeTestCaseFragment(ConsumeTagContents(testCasesContents, "funcName", ref huh), entrypointClassName);
            var display = SanitizeTestCaseFragment(ConsumeTagContents(testCasesContents, "display", ref huh), entrypointClassName);
            var displayRes = SanitizeTestCaseFragment(ConsumeTagContents(testCasesContents, "displayRes", ref huh), entrypointClassName);
            var isPublic = SanitizeTestCaseFragment(ConsumeTagContents(testCasesContents, "public", ref huh), entrypointClassName) == "true";
            
            testCases.Add(new TestCaseDto
            {
                TestCaseId = testCaseId,
                Display = isPublic ? display : "",
                DisplayRes = isPublic ? displayRes : "",
                IsPublic = isPublic
            });
        }
        
    }
}