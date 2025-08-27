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
public class ExecutorFileOperationHandler(UserSolutionData userSolutionData)
{
    public static readonly string JavaGsonImport = "import com.google.gson.Gson;\n";
    private const string TestCaseIdStartFlag = "tc_id:";
    private const int UuidLength = 36;

    public async Task<List<TestResultDto>> ReadTestingResults()
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
    
    public async Task<int> ReadExecutionTime()
    {
        var executionTimeRaw = await File.ReadAllTextAsync(GetTimingLogFilePath(userSolutionData.ExecutionId));
        var executionTimeSanitized = executionTimeRaw.Replace(GetExecutionSigningString(userSolutionData.SigningKey, SigningType.Time), "");
        return int.Parse(executionTimeSanitized.Trim());
    }

    public async Task<string> ReadExecutionStandardOut()
    {
        var readAllTextAsync = await File.ReadAllTextAsync(GetStdOutLogFilePath(userSolutionData.ExecutionId));
        return readAllTextAsync;
    }
    
    public void InsertTestCases(List<TestCase> testCases)
    {
        var gsonInstanceName = $"{GetHelperVariableNamePrefix()}_gson";
        var gsonVariableInitialization = $"Gson {gsonInstanceName} = new Gson();\n";
        InsertAtEndOfMainMethod(gsonVariableInitialization);
        
        foreach (var testCase in testCases)
        {
            InsertAtEndOfMainMethod(testCase.TestInput);
            InsertAtEndOfMainMethod(CreateComparingStatement(testCase, gsonInstanceName));
        }
    }

    public void InsertTiming()
    {
        var timingStartVariableName = $"{GetHelperVariableNamePrefix()}_start";
        var timingEndVariableName = $"{GetHelperVariableNamePrefix()}_end";
        
        InsertAtStartOfMainMethod(GetTimingVariable(timingStartVariableName));
        InsertAtEndOfMainMethod(GetTimingVariable(timingEndVariableName));
        InsertAtEndOfMainMethod(CreateSignedPrintStatement($"({timingEndVariableName} - {timingStartVariableName})", SigningType.Time));
    }
    
    private string GetHelperVariableNamePrefix()
    {
        var gsonInstanceName = new StringBuilder("a"); // sometimes guids start with numbers, java variables names on the other hand cannot
        gsonInstanceName.Append(userSolutionData.ExerciseId.ToString().Replace("-", ""));
        return gsonInstanceName.ToString();
    }

    private void InsertAtEndOfMainMethod(string codeToBeInserted)
    {
        userSolutionData.FileContents.Insert(userSolutionData.MainMethod!.MethodFileEndIndex, codeToBeInserted);
        userSolutionData.MainMethod!.MethodFileEndIndex += codeToBeInserted.Length;
    }
    
    private void InsertAtStartOfMainMethod(string codeToBeInserted)
    {
        userSolutionData.FileContents.Insert(userSolutionData.MainMethod!.MethodFileBeginIndex + 1, codeToBeInserted);
        userSolutionData.MainMethod!.MethodFileEndIndex += codeToBeInserted.Length;
    }
    
    private string CreateSignedPrintStatement(string printContents, SigningType signingType)
    {
        return $"System.out.println(\"{GetExecutionSigningString(userSolutionData.SigningKey, signingType)}\" + {printContents});\n";
    }
    
    private string CreateComparingStatement(TestCase testCase, string gsonInstanceName)
    {
        return CreateSignedPrintStatement($"\" tc_id:{testCase.Id} \" + {gsonInstanceName}.toJson({testCase.ExpectedOutput}).equals({gsonInstanceName}.toJson({testCase.FuncName}({testCase.Call})))",  SigningType.Answer);
    }

    private string GetTimingVariable(string variableName)
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