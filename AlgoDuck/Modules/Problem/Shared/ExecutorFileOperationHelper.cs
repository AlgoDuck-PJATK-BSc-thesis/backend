using System.Text;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuckShared;

namespace AlgoDuck.Modules.Problem.Shared;

internal enum SigningType
{
    Time, Answer
}
public class TestResultDto
{
    public string TestId { get; set; } = string.Empty;
    public bool IsTestPassed { get; set; } = false;
}

public class SubmitExecuteResponse
{
    public string StdOutput { get; set; } = string.Empty;
    public string StdError { get; set; } = string.Empty;
    public List<TestResultDto> TestResults { get; set; } = [];
    public int ExecutionTime { get; set; } = 0;
    public SubmitExecuteRequestRabbitStatus Status { get; set; }
}

public class ExecutorFileOperationHelper
{
    private const string TestCaseIdStartFlag = "tc_id:";
    private const string AnswerControlSymbol = "-answ:";
    private const string TimeControlSymbol = "-time:";
    private const int UuidLength = 36;
    private const int SigningKeyStringLen = UuidLength + 4; // "ctr-70fcae06-b1ac-453b-b0a0-57812ba86cf4". 4 chars for "ctr-" + uuid length
    private const string JavaGsonImport = "import com.google.gson.Gson;\n"; 
    
    public required UserSolutionData UserSolutionData { get; set; }

    internal SubmitExecuteResponse ParseVmOutput(ExecutionResponseRabbit vmOutput)
    {
        List<TestResultDto> testResults = [];
        var executionTime = 0;
        var javaStdOut = new StringBuilder();
        
        foreach (var line in vmOutput.Out.ReplaceLineEndings().Split(Environment.NewLine))
        {
            if (line.Contains($"ctr-{UserSolutionData.SigningKey}"))
            {
                switch (line.Substring(SigningKeyStringLen, AnswerControlSymbol.Length))
                {
                    case AnswerControlSymbol:
                        testResults.Add(ParseTestCaseResult(line));
                        break;
                    case TimeControlSymbol:
                        executionTime = ParseTimingOutput(line);
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

        return new SubmitExecuteResponse
        {
            ExecutionTime = executionTime,
            StdError = vmOutput.Err,
            StdOutput = javaStdOut.ToString(),
            TestResults = testResults,
            Status = vmOutput.Status
        };
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

    public void InsertTestCases(List<TestCaseJoined> testCases, string mainClassName)
    {
        var gsonInstanceName = $"{GetHelperVariableNamePrefix()}_gson";
        var gsonVariableInitialization = $"Gson {gsonInstanceName} = new Gson();\n";
        InsertAtEndOfMainMethod(gsonVariableInitialization);
        
        testCases.ForEach(t => ArrangeTestCase(t, mainClassName));
        testCases.ForEach(ActTestCase);
        testCases.ForEach(t => AssertTestCase(t, gsonInstanceName));
    }

    public void ArrangeTestCase(TestCaseJoined testCase, string mainClassName)
    {
        InsertAtEndOfMainMethod(testCase.Setup.Replace("${ENTRYPOINT_CLASS_NAME}", mainClassName));
    }

    internal void ActTestCase(TestCaseJoined testCase)
    {
        var args = testCase.Call.Length == 0 ? "" : string.Join(",", testCase.Call);
        InsertAtEndOfMainMethod($"var {ConvertGuidToJavaVariableName(testCase.TestCaseId)} = {testCase.CallFunc}({args});");
    }

    private string ConvertGuidToJavaVariableName(Guid guid)
    {
        return $"a{guid.ToString().Replace('-', '_')}";
    }

    private void AssertTestCase(TestCaseJoined testCase, string gsonInstanceName)
    {
        InsertAtEndOfMainMethod(CreateSignedPrintStatement(
            $"\" tc_id:{testCase.TestCaseId} \" + {gsonInstanceName}.toJson({testCase.Expected}).equals({gsonInstanceName}.toJson({ConvertGuidToJavaVariableName(testCase.TestCaseId)}))",
            SigningType.Answer));
    }

    public void InsertTiming()
    {
        var timingStartVariableName = $"{GetHelperVariableNamePrefix()}_start";
        var timingEndVariableName = $"{GetHelperVariableNamePrefix()}_end";
        
        InsertAtStartOfMainMethod(GetTimingVariable(timingStartVariableName));
        InsertAtEndOfMainMethod(GetTimingVariable(timingEndVariableName));
        InsertAtEndOfMainMethod(CreateSignedPrintStatement($"({timingEndVariableName} - {timingStartVariableName})", SigningType.Time));
    }

    internal void InsertGsonImport()
    {
        InsertAtStartOfFile(JavaGsonImport);
    }

    private void InsertAtStartOfFile(string codeToBeInserted)
    {
        UserSolutionData.FileContents.Insert(0, codeToBeInserted);
        UserSolutionData.MainMethod!.MethodFileEndIndex += codeToBeInserted.Length;
    }
    
    private string GetHelperVariableNamePrefix()
    {
        var gsonInstanceName = new StringBuilder("a"); // sometimes guids start with numbers, java variables names on the other hand cannot
        gsonInstanceName.Append(Guid.NewGuid().ToString().Replace("-", ""));
        return gsonInstanceName.ToString();
    }

    private void InsertAtEndOfMainMethod(string codeToBeInserted)
    {
        UserSolutionData.FileContents.Insert(UserSolutionData.MainMethod!.MethodFileEndIndex, codeToBeInserted);
        UserSolutionData.MainMethod!.MethodFileEndIndex += codeToBeInserted.Length;
    }
    
    private void InsertAtStartOfMainMethod(string codeToBeInserted)
    {
        UserSolutionData.FileContents.Insert(UserSolutionData.MainMethod!.MethodFileBeginIndex + 1, codeToBeInserted);
        UserSolutionData.MainMethod!.MethodFileEndIndex += codeToBeInserted.Length;
    }
    
    private string CreateSignedPrintStatement(string printContents, SigningType signingType)
    {
        return $"System.out.println(\"{GetExecutionSigningString(UserSolutionData.SigningKey, signingType)}\" + {printContents});\n";
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