using System.Text;
using System.Text.Json;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Shared.Types;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Http;
using AlgoDuckShared;

namespace AlgoDuck.Modules.Problem.Shared;


public class ExecutorFileOperationHelper
{
    private static class ControlSymbols
    {
        public const string TestCaseIdPrefix = "tc_id:";
        public const string Answer = "-answ:";
        public const string Time = "-time:";
        public const string ControlPrefix = "ctr-";
    }

    private static class Lengths
    {
        public const int Uuid = 36;
        public const int SigningKey = Uuid + 4; // "ctr-" + UUID
    }

    private static class JavaCode
    {
        public const string GsonImport = "import com.google.gson.Gson;\n";
        
        public const string NormalizerClassName = "Normalizer7423d798e0454599918af7e204faf588";
        
        public const string NormalizerCode = """
            final class Normalizer7423d798e0454599918af7e204faf588 {
                public static Comparable<?> normalize(Object obj, boolean orderMatters) {
                    if (obj == null) return null;
                
                    if (obj instanceof Number || obj instanceof String || obj instanceof Boolean) {
                        return obj.toString();
                    }
                
                    if (obj.getClass().isArray()) {
                        java.util.List<String> normalized = new java.util.ArrayList<>();
                        int len = java.lang.reflect.Array.getLength(obj);
                        for (int i = 0; i < len; i++) {
                            normalized.add(normalize(java.lang.reflect.Array.get(obj, i), orderMatters).toString());
                        }
                        if (!orderMatters) {
                            java.util.Collections.sort(normalized);
                        }
                        return normalized.toString();
                    }
                
                    if (obj instanceof java.util.Collection<?> coll) {
                        java.util.List<String> normalized = coll.stream()
                                .map(e -> normalize(e, orderMatters).toString())
                                .collect(java.util.stream.Collectors.toList());
                        if (!orderMatters) {
                            java.util.Collections.sort(normalized);
                        }
                        return normalized.toString();
                    }
                
                    return new com.google.gson.Gson().toJson(obj);
                }
            }
            """;
    }

    public required UserSolutionData UserSolutionData { get; set; }

    internal SubmitExecuteResponse ParseVmOutput(ExecutionResponseRabbit vmOutput)
    {
        var testResults = new List<TestResultDto>();
        var javaStdOut = new StringBuilder();
        var signingKeyMarker = $"{ControlSymbols.ControlPrefix}{UserSolutionData.SigningKey}";

        foreach (var line in vmOutput.Out.ReplaceLineEndings().Split(Environment.NewLine))
        {
            if (line.Contains(signingKeyMarker))
            {
                ProcessControlLine(line, testResults);
            }
            else
            {
                javaStdOut.Append(line);
            }
        }
        Console.WriteLine("finished parse");


        return new SubmitExecuteResponse
        {
            StdError = vmOutput.Err,
            StdOutput = javaStdOut.ToString(),
            ExecutionEndTimeNs = vmOutput.EndNs,
            ExecutionStartTimeNs = vmOutput.StartNs,
            ExecutionExitCode = vmOutput.ExitCode,
            JvmMemoryPeakKb = vmOutput.MaxMemoryKb,
            TestResults = testResults,
            Status = vmOutput.Status
        };
    }

    private static void ProcessControlLine(string line, List<TestResultDto> testResults)
    {
        var controlType = line.Substring(Lengths.SigningKey, ControlSymbols.Answer.Length);

        switch (controlType)
        {
            case ControlSymbols.Answer:
                var testCaseResult = ParseTestCaseResult(line).Map(tesCase =>
                {
                    testResults.Add(tesCase);
                    return tesCase;
                });
                break;
            default:
                throw new MangledControlSymbolException();
        }
    }

    private static Result<TestResultDto, ErrorObject<string>> ParseTestCaseResult(string line)
    {
        var idStartIndex = line.IndexOf(ControlSymbols.TestCaseIdPrefix, StringComparison.Ordinal) 
                         + ControlSymbols.TestCaseIdPrefix.Length;
        try
        {
            var testCaseId = Guid.Parse(line.AsSpan(idStartIndex, Lengths.Uuid));
            var testResultRaw = line[(idStartIndex + Lengths.Uuid)..];
            return Result<TestResultDto, ErrorObject<string>>.Ok(new TestResultDto
            {
                IsTestPassed = testResultRaw.Trim() == "true",
                TestId = testCaseId
            });            
        }
        catch (Exception e)
        {
            return Result<TestResultDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest("Could not parse test result"));
        }

    }

    public void InsertTestCases(List<TestCaseJoined> testCases, CodeAnalysisResult analysisResult)
    {
        UserSolutionData.FileContents.Append(JavaCode.NormalizerCode);
        
        NormalizeTestCaseVariables(testCases);

        NormalizeTestCaseMethodCalls(testCases, analysisResult);

        foreach (var testCase in testCases)
        {
            ArrangeTestCase(testCase, analysisResult.MainClassName);
            ActTestCase(testCase);
            AssertTestCase(testCase);
        }
    }
    
    public void InsertTestCaseForExecution(CodeAnalysisResult analysisResult, params TestCaseJoined[] testCases)
    {
        NormalizeTestCaseVariables(testCases);
        NormalizeTestCaseMethodCalls(testCases, analysisResult);
    
        foreach (var testCaseJoined in testCases)
        {
            ArrangeTestCase(testCaseJoined, analysisResult.MainClassName);
            ActTestCase(testCaseJoined);
        }
    }

    private static void NormalizeTestCaseMethodCalls(IEnumerable<TestCaseJoined> testCases, CodeAnalysisResult analysisResult)
    {
        foreach (var testCaseJoined in testCases)
        {
            testCaseJoined.CallFunc =
                testCaseJoined.CallFunc.Replace("${ENTRYPOINT_CLASS_NAME}", analysisResult.MainClassName);
            testCaseJoined.Setup = testCaseJoined.Setup.Replace("${ENTRYPOINT_CLASS_NAME}", analysisResult.MainClassName);
            for (var i = 0; i < testCaseJoined.Call.Length; i++)
            {
                testCaseJoined.Call[i] = testCaseJoined.Call[i]
                    .Replace("${ENTRYPOINT_CLASS_NAME}", analysisResult.MainClassName);
            }
        }
    }
    private static void NormalizeTestCaseVariables(IEnumerable<TestCaseJoined> testCases)
    {
        foreach (var (testCase, testCaseIndex) in testCases.Select((tc, i) => (tc, i)))
        {
            for (var varIndex = 0; varIndex < testCase.VariableCount; varIndex++)
            {
                var newVariableName = GenerateUniqueVariableName();
                var interpolationPattern = $"{{tc_{testCaseIndex}_var_{varIndex}}}";

                testCase.Setup = testCase.Setup.Replace(interpolationPattern, newVariableName);
                testCase.Expected = testCase.Expected.Replace(interpolationPattern, newVariableName);
                
                for (var i = 0; i < testCase.Call.Length; i++)
                {
                    testCase.Call[i] = testCase.Call[i].Replace(interpolationPattern, newVariableName);
                }
            }
        }
    }

    private void ArrangeTestCase(TestCaseJoined testCase, string mainClassName)
    {
        var setup = testCase.Setup.Replace("${ENTRYPOINT_CLASS_NAME}", mainClassName);
        InsertAtEndOfMainMethod(setup);
    }

    private void ActTestCase(TestCaseJoined testCase)
    {
        var args = testCase.Call.Length == 0 ? "" : string.Join(",", testCase.Call);
        var variableName = GuidToJavaVariableName(testCase.TestCaseId);
        
        InsertAtEndOfMainMethod($"var {variableName} = {testCase.CallFunc}({args});");
    }

    private void AssertTestCase(TestCaseJoined testCase)
    {
        var variableName = GuidToJavaVariableName(testCase.TestCaseId);
        var orderMatters = testCase.OrderMatters.ToString().ToLowerInvariant();
        
        var assertExpression = 
            $"\" {ControlSymbols.TestCaseIdPrefix}{testCase.TestCaseId} \" + " +
            $"{JavaCode.NormalizerClassName}.normalize({testCase.Expected}, {orderMatters})" +
            $".equals({JavaCode.NormalizerClassName}.normalize({variableName}, {orderMatters}))";

        InsertAtEndOfMainMethod(CreateSignedPrintStatement(assertExpression, SigningType.Answer));
    }


    public void InsertTiming()
    {
        var startVar = $"{GenerateUniqueVariableName()}_start";
        var endVar = $"{GenerateUniqueVariableName()}_end";

        InsertAtStartOfMainMethod(CreateTimingVariable(startVar));
        InsertAtEndOfMainMethod(CreateTimingVariable(endVar));
        InsertAtEndOfMainMethod(CreateSignedPrintStatement($"({endVar} - {startVar})", SigningType.Time));
    }

    private static string CreateTimingVariable(string variableName) => 
        $"long {variableName} = System.currentTimeMillis();\n";


    private void InsertAtStartOfFile(string code)
    {
        UserSolutionData.FileContents.Insert(0, code);
        UserSolutionData.MainMethod!.MethodFileEndIndex += code.Length;
    }

    private void InsertAtEndOfMainMethod(string code)
    {
        UserSolutionData.FileContents.Insert(
            UserSolutionData.MainMethod!.MethodFileEndIndex /*TODO: here 1 - 1*/, 
            code);
        UserSolutionData.MainMethod.MethodFileEndIndex += code.Length;
    }

    private void InsertAtStartOfMainMethod(string code)
    {
        UserSolutionData.FileContents.Insert(
            UserSolutionData.MainMethod!.MethodFileBeginIndex + 1, 
            code);
        UserSolutionData.MainMethod.MethodFileEndIndex += code.Length;
    }

    private static string GenerateUniqueVariableName()
    {
        // Prefix with 'a' because Java variable names cannot start with numbers
        return $"a{Guid.NewGuid():N}";
    }

    private static string GuidToJavaVariableName(Guid guid) => 
        $"a{guid.ToString().Replace('-', '_')}";

    private string CreateSignedPrintStatement(string content, SigningType signingType)
    {
        var signingString = GetSigningString(UserSolutionData.SigningKey, signingType);
        return $"System.out.println(\"{signingString}\" + {content});\n";
    }

    private static string GetSigningString(Guid signingKey, SigningType signingType)
    {
        var typeFlag = signingType switch
        {
            SigningType.Answer => "answ",
            SigningType.Time => "time",
            _ => throw new ArgumentOutOfRangeException(nameof(signingType))
        };

        return $"{ControlSymbols.ControlPrefix}{signingKey}-{typeFlag}: ";
    }
}