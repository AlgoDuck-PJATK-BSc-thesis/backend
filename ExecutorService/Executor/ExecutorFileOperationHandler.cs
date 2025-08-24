using System.Diagnostics;
using System.Text;
using ExecutorService.Executor.Dtos;
using ExecutorService.Executor.Models;
using ExecutorService.Executor.Types;

namespace ExecutorService.Executor;

internal enum SigningType
{
    Time, PowerOff, Answer
}
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
            var testResLineSanitized = testResLine.Replace(GetExecutionSigningString(userSolutionData.SigningKey, SigningType.Answer), "");
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
        var executionTimeSanitized = executionTimeRaw.Replace(GetExecutionSigningString(userSolutionData.SigningKey, SigningType.Time), "");
        return int.Parse(executionTimeSanitized.Trim());
    }

    public static async Task<string> ReadExecutionStandardOut(UserSolutionData userSolutionData)
    {
        var readAllTextAsync = await File.ReadAllTextAsync(GetStdOutLogFilePath(userSolutionData.ExecutionId));
        return readAllTextAsync;
    }
    
    public static void InsertTestCases(UserSolutionData userSolutionData, List<TestCase> testCases)
    {
        var gsonInstanceName = $"{GetHelperVariableNamePrefix(userSolutionData)}_gson";
        var gsonVariableInitialization = $"Gson {gsonInstanceName} = new Gson();\n";
        InsertAtEndOfMainMethod(userSolutionData, gsonVariableInitialization);
        
        foreach (var testCase in testCases)
        {
            InsertAtEndOfMainMethod(userSolutionData, testCase.TestInput);
            InsertAtEndOfMainMethod(userSolutionData, CreateComparingStatement(userSolutionData, testCase, gsonInstanceName));
        }
    }

    public static void InsertTiming(UserSolutionData userSolutionData)
    {
        var timingStartVariableName = $"{GetHelperVariableNamePrefix(userSolutionData)}_start";
        var timingEndVariableName = $"{GetHelperVariableNamePrefix(userSolutionData)}_end";
        
        InsertAtStartOfMainMethod(userSolutionData, GetTimingVariable(timingStartVariableName));
        InsertAtEndOfMainMethod(userSolutionData, GetTimingVariable(timingEndVariableName));
        InsertAtEndOfMainMethod(userSolutionData, CreateSignedPrintStatement(userSolutionData, $"({timingEndVariableName} - {timingStartVariableName})", SigningType.Time));
    }
    
    private static string GetHelperVariableNamePrefix(UserSolutionData userSolutionData)
    {
        var gsonInstanceName = new StringBuilder("a"); // sometimes guids start with numbers, java variables names on the other hand cannot
        gsonInstanceName.Append(userSolutionData.ExerciseId.ToString().Replace("-", ""));
        return gsonInstanceName.ToString();
    }

    private static void InsertAtEndOfMainMethod(UserSolutionData userSolutionData, string codeToBeInserted)
    {
        userSolutionData.FileContents.Insert(userSolutionData.MainMethod!.MethodFileEndIndex, codeToBeInserted);
        userSolutionData.MainMethod!.MethodFileEndIndex += codeToBeInserted.Length;
    }
    
    private static void InsertAtStartOfMainMethod(UserSolutionData userSolutionData, string codeToBeInserted)
    {
        userSolutionData.FileContents.Insert(userSolutionData.MainMethod!.MethodFileBeginIndex + 1, codeToBeInserted);
        userSolutionData.MainMethod!.MethodFileEndIndex += codeToBeInserted.Length;
    }
    
    private static string CreateSignedPrintStatement(UserSolutionData userSolutionData, string printContents, SigningType signingType)
    {
        return $"System.out.println(\"{GetExecutionSigningString(userSolutionData.SigningKey, signingType)}\" + {printContents});\n";
    }
    
    private static string CreateComparingStatement(UserSolutionData userSolutionData, TestCase testCase, string gsonInstanceName)
    {
        return CreateSignedPrintStatement(userSolutionData,$"\" tc_id:{testCase.Id} \" + {gsonInstanceName}.toJson({testCase.ExpectedOutput}).equals({gsonInstanceName}.toJson({testCase.FuncName}({testCase.Call})))",  SigningType.Answer);
    }

    private static string GetTimingVariable(string variableName)
    {
        return $"long {variableName} = System.currentTimeMillis();\n";
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

    private static string GetExecutionSigningString(Guid signingKey, SigningType signingType)
    {
        var signingTypeFlag = signingType switch
        {
            SigningType.Answer => "ans",
            SigningType.Time => "time",
            SigningType.PowerOff => "pof",
            _ => throw new ArgumentOutOfRangeException(nameof(signingType), signingType, null)
        };
        
        return $"ctr-{signingKey}-{signingTypeFlag}: ";
    }
}