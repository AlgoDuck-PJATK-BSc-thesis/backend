using System.Text;
using ExecutorService.Executor.Dtos;
using ExecutorService.Executor.Models;
using ExecutorService.Executor.Types;

namespace ExecutorService.Executor;

public static class ExecutorFileOperationHandler
{
    public static readonly string JavaGsonImport = "import com.google.gson.Gson;\n";
    private const string TestCaseIdStartFlag = "tc_id:";
    private const int UuidLength = 36;

    public static async Task<List<TestResultDto>> ReadTestingResults(UserSolutionData userSolutionData)
    {
        List<TestResultDto> parsedTestCases = [];
        var testResultsRaw = await File.ReadAllTextAsync(GetTestResultLogFilePath(userSolutionData.ExecutionId));
        if (string.IsNullOrEmpty(testResultsRaw)) return parsedTestCases;
        var testResLines = testResultsRaw.ReplaceLineEndings().Trim().Split("\n");
        foreach (var testResLine in testResLines)
        {
            var testResLineSanitized = testResLine.Replace(GetExecutionSigningString(userSolutionData.SigningKey), "");
            var idStartIndex = testResLineSanitized.IndexOf(TestCaseIdStartFlag, StringComparison.Ordinal) + TestCaseIdStartFlag.Length;
            var idEndIndex = idStartIndex + UuidLength;
            var testCaseId = testResLineSanitized.Substring(idStartIndex, UuidLength);
            var testCaseRes = testResLineSanitized[idEndIndex..].Trim() == "true";
            parsedTestCases.Add(new TestResultDto
            {
                TestId = testCaseId,
                IsTestPassed = testCaseRes
            });
        }

        return parsedTestCases;
    }
    
    // TODO first of all use something other than "time" for more precise measurements, secondly create and move this to some FileOperationsHandle class. Also generally make this not bad
    public static async Task<int> ReadExecutionTime(UserSolutionData userSolutionData)
    {
        var executionTimeRaw = await File.ReadAllTextAsync(GetTimingLogFilePath(userSolutionData.ExecutionId));
        var executionTimeParsed = double.Parse(executionTimeRaw.Split(" ").ElementAt(2).Replace("s", ""));
        return (int)(executionTimeParsed * 1000);
    }

    public static async Task<string> ReadExecutionStandardOut(UserSolutionData userSolutionData)
    {
        return await File.ReadAllTextAsync(GetStdOutLogFilePath(userSolutionData.ExecutionId));
    }
    
    public static void InsertTestCases(UserSolutionData userSolutionData, List<TestCase> testCases)
    {
        var gsonInstanceName = GetGsonInstanceName(userSolutionData);
        
        var testCaseInsertBuilder = new StringBuilder();
        testCaseInsertBuilder.Append($"Gson {gsonInstanceName} = new Gson();\n");
        
        foreach (var testCase in testCases)
        {
            testCaseInsertBuilder.Append(testCase.TestInput);
            testCaseInsertBuilder.Append(CreateComparingStatement(userSolutionData, testCase, gsonInstanceName));
        }
        
        userSolutionData.FileContents.Insert(userSolutionData.MainMethod!.MethodFileEndIndex, testCaseInsertBuilder);
    }
    
    private static string GetGsonInstanceName(UserSolutionData userSolutionData)
    {
        var gsonInstanceName = new StringBuilder("a"); // sometimes guids start with numbers, java variables names on the other hand cannot
        gsonInstanceName.Append(userSolutionData.ExerciseId.ToString().Replace("-", ""));
        return gsonInstanceName.ToString();
    }

    private static string CreateComparingStatement(UserSolutionData userSolutionData, TestCase testCase, string gsonInstanceName)
    {
        return $"System.out.println(\"{GetExecutionSigningString(userSolutionData.SigningKey)}\" + \" tc_id:{testCase.Id} \" + {gsonInstanceName}.toJson({testCase.ExpectedOutput}).equals({gsonInstanceName}.toJson({testCase.FuncName}({testCase.Call}))));\n";
    }

    private static string GetStdOutLogFilePath(Guid executionId)
    {
        return $"/tmp/{executionId}-OUT-LOG.log";
    }

    private static string GetTimingLogFilePath(Guid executionId)
    {
        return $"/tmp/{executionId}-TIME-LOG.log";
    }

    private static string GetStdErrLogFilePath(Guid executionId)
    {
        return $"/tmp/{executionId}-ERR-LOG.log";
    }

    private static string GetTestResultLogFilePath(Guid executionId)
    {
        return $"/tmp/{executionId}-ANSW-LOG.log";
    }

    private static string GetExecutionSigningString(Guid signingKey)
    {
        return $"ctr-{signingKey}-ans: ";
    }
}