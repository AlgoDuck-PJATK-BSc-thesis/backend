namespace ExecutorService.Executor._ExecutorUtils;

public class TestCase(string testInput, string expectedOutput)
{
    public string TestInput => testInput;

    public string ExpectedOutput => expectedOutput;

    public static TestCase[] ParseTestCases(string testCases)
    {
        var testCaseList = new List<TestCase>();
        for (var i = 0; i < testCases.Length;)
        {
            var endOfTest = testCases.IndexOf('<', i);
            var testInput = testCases.Substring(i, endOfTest - i);
            i = endOfTest + 2;
            
            var endOfCorr = testCases.IndexOf("<<", i, StringComparison.Ordinal);
            var testOutput = testCases.Substring(i, endOfCorr - i);
            i = endOfCorr + 3;
            
            testCaseList.Add(new TestCase(testInput, testOutput));
        }

        return testCaseList.ToArray();
    }
    
    //TODO cool funky version below boring practical version above (marked todo because it pops out more)
    IEnumerable<TestCase> EnumerateTestCases(string testCases)
    {
        for (var i = 0; i < testCases.Length;)
        {
            var endOfTest = testCases.IndexOf('<', i);
            var str1 = testCases.Substring(i, endOfTest - i);
            i = endOfTest + 2;

            var endOfCorr = testCases.IndexOf("<<", i, StringComparison.Ordinal);
            var str2 = testCases.Substring(i, endOfCorr - i);
            i = endOfCorr + 3;
            yield return new TestCase(str1, str2);
        }
    }
}