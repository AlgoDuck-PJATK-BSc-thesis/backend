using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.Dtos;
using ExecutorService.Executor.Types;
// ReSharper disable ReplaceSubstringWithRangeIndexer

namespace ExecutorService.Executor;

internal enum SigningType
{
    Time, Answer
}
public class ExecutorFileOperationHandler(UserSolutionData userSolutionData)
{
    private const string TestCaseIdStartFlag = "tc_id:";
    private const string AnswerControlSymbol = "-answ:";
    private const string TimeControlSymbol = "-time:";
    private const int UuidLength = 36;
    private const int SigningKeyStringLen = UuidLength + 4; // "ctr-70fcae06-b1ac-453b-b0a0-57812ba86cf4". 4 chars for "ctr-" + uuid length
    private static JsonSerializerOptions _serializerOptions = new(){PropertyNameCaseInsensitive = true};


    internal ExecuteResultDto ParseVmOutput(VmExecutionResponse vmOutput)
    {
        var executeResultDto = new ExecuteResultDto
        {
            StdError = vmOutput.Err!,
        };

        var javaStdOut = new StringBuilder();
        
        foreach (var line in vmOutput.Out!.ReplaceLineEndings().Split(Environment.NewLine))
        {
            if (line.Contains($"ctr-{userSolutionData.SigningKey}"))
            {
                switch (line.Substring(SigningKeyStringLen, AnswerControlSymbol.Length))
                {
                    case "-answ:":
                        executeResultDto.TestResults.Add(ParseTestCaseResult(line));
                        break;
                    case "-time:":
                        executeResultDto.ExecutionTime = ParseTimingOutput(line);
                        break;
                    default:
                        throw new MangledControlSymbolException();
                }
            }
            else
            {
                javaStdOut.Append(line);
            }
        }

        executeResultDto.StdOutput = javaStdOut.ToString();
        return executeResultDto;
    }
    
    private TestResultDto ParseTestCaseResult(string line)
    {
        var idStartIndex = line.IndexOf(TestCaseIdStartFlag, StringComparison.Ordinal) + TestCaseIdStartFlag.Length;
        var testCaseId = line.Substring(idStartIndex, UuidLength);
        var testResultRaw = line.Substring(idStartIndex + UuidLength);
        var testResult = testResultRaw.Trim() == "true";
        return new TestResultDto
        {
            IsTestPassed = testResult,
            TestId = testCaseId,
        };
    }

    private int ParseTimingOutput(string line)
    {
        var timeInMillis = line.Substring(line.IndexOf("-time:", StringComparison.Ordinal) + TimeControlSymbol.Length);
        return int.Parse(timeInMillis.Trim());
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

    private static string GetExecutionSigningString(Guid signingKey, SigningType signingType)
    {
        var signingTypeFlag = signingType switch
        {
            SigningType.Answer => "answ",
            SigningType.Time => "time",
            _ => throw new ArgumentOutOfRangeException(nameof(signingType), signingType, null)
        };
        
        return $"ctr-{signingKey}-{signingTypeFlag}: ";
    }
}