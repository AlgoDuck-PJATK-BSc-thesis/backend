using System.Text;
using System.Text.Json;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.CreateProblem;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;
using AlgoDuck.Shared.Analyzer.AstBuilder.Lexer;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertUtils;

public interface ITestCaseProcessor
{
    Result<List<TestCaseJoined>, ErrorObject<string>> ProcessTestCases(
        List<TestCaseDto> testCases,
        AnalyzerSimple analyzer,
        CodeAnalysisResult analysisResult);
}

public class TestCaseProcessor : ITestCaseProcessor
{
    public Result<List<TestCaseJoined>, ErrorObject<string>> ProcessTestCases(
        List<TestCaseDto> testCases,
        AnalyzerSimple analyzer,
        CodeAnalysisResult analysisResult)
    {
        var resolveResult = ResolveFunctionCalls(testCases, analyzer, analysisResult);
        if (resolveResult.IsErr)
            return Result<List<TestCaseJoined>, ErrorObject<string>>.Err(resolveResult.AsErr!);
        
        var arranges = testCases
            .Select(SanitizeArrangeBlock)
            .ToList();
        
        var joinedTestCases = testCases
            .Select((tc, i) => BuildJoinedTestCase(tc, arranges[i]))
            .ToList();

        return Result<List<TestCaseJoined>, ErrorObject<string>>.Ok(joinedTestCases);
    }
    

    private Result<bool, ErrorObject<string>> ResolveFunctionCalls(
        List<TestCaseDto> testCases,
        AnalyzerSimple analyzer,
        CodeAnalysisResult analysisResult)
    {
        foreach (var testCase in testCases)
        {
            var qualifiedParts = testCase.CallMethod.QualifiedName.Split('.');
            var resolveResult = analyzer.RecursiveResolveFunctionCall(
                analysisResult.Main.FuncScope!.OwnScope,
                qualifiedParts);

            if (resolveResult.IsErr)
            {
                return Result<bool, ErrorObject<string>>.Err(
                    ErrorObject<string>.BadRequest($"Cannot resolve function: {testCase.CallMethod.MethodName}"));
            }

            testCase.ResolvedFunctionCall = resolveResult.AsT0.Replace(analysisResult.MainClassName, "${ENTRYPOINT_CLASS_NAME}");
        }

        return Result<bool, ErrorObject<string>>.Ok(true);
    }

    private TestCaseArrangeSanitized SanitizeArrangeBlock(TestCaseDto testCase, int testCaseIndex)
    {
        var arrangeContent = Encoding.UTF8.GetString(Convert.FromBase64String(testCase.ArrangeB64));
        var arrangeAnalyzer = new AnalyzerSimple(new StringBuilder(arrangeContent));
        var arrangeAnalysis = arrangeAnalyzer.AnalyzeUserCode(ExecutionStyle.Execution);

        var declaredVariables = GetDeclaredVariables(arrangeAnalyzer, arrangeAnalysis);
        var strippedContent = ExtractMainMethodContent(arrangeContent, arrangeAnalysis);
        var variableMappings = CreateVariableMappings(declaredVariables, testCaseIndex);

        variableMappings[arrangeAnalysis.MainClassName] = "${ENTRYPOINT_CLASS_NAME}";
        
        var sanitizedContent = ApplySubstitutions(strippedContent, variableMappings);

        variableMappings.Remove(arrangeAnalysis.MainClassName);

        return new TestCaseArrangeSanitized
        {
            ArrangeStripped = sanitizedContent,
            VariableMappings = variableMappings
        };
    }

    private static List<AstNodeScopeMemberVar> GetDeclaredVariables(AnalyzerSimple analyzer, CodeAnalysisResult analysis)
    {
        var variables = new List<AstNodeScopeMemberVar>();
        analyzer.GetAllVariablesAccessibleFromScope(
            analysis.Main.FuncScope!.OwnScope,
            variables);
        return variables;
    }

    private static string ExtractMainMethodContent(string content, CodeAnalysisResult analysis)
    {
        if (analysis.MainMethodIndices == null)
            return content;

        var startIndex = analysis.MainMethodIndices.MethodFileBeginIndex + 1;
        var length = analysis.MainMethodIndices.MethodFileEndIndex 
                   - analysis.MainMethodIndices.MethodFileBeginIndex - 1;

        return content.Substring(startIndex, length);
    }

    private static (int, int) ExtractMainMethodLengthAndStartingIndex(CodeAnalysisResult analysis)
    {
        if (analysis.MainMethodIndices == null)
            return (0, 0);
        
        var startIndex = analysis.MainMethodIndices.MethodFileBeginIndex + 1;
        var length = analysis.MainMethodIndices.MethodFileEndIndex 
                     - analysis.MainMethodIndices.MethodFileBeginIndex - 1;
        
        return (startIndex, length);
    }

    private static Dictionary<string, string> CreateVariableMappings(List<AstNodeScopeMemberVar> variables, int testCaseIndex)
    {
        return variables
            .Select((v, i) => (Variable: v, Index: i))
            .Where(x => x.Variable.Identifier?.Value != null)
            .ToDictionary(
                x => x.Variable.Identifier!.Value!,
                x => $"{{tc_{testCaseIndex}_var_{x.Index}}}");
    }

    private static string ApplySubstitutions(string content, Dictionary<string, string> mappings)
    {
        var tokens = LexerSimple.Tokenize(content);
        var replacements = new List<(int Position, int Length, string NewValue)>();

        foreach (var token in tokens)
        {
            if (token.Type == TokenType.Ident &&
                mappings.TryGetValue(token.Value!, out var newName))
            {
                replacements.Add((token.FilePos, token.Value!.Length, newName));
            }
        }

        var result = new StringBuilder(content);
        foreach (var (pos, len, newVal) in replacements.OrderByDescending(r => r.Position))
        {
            result.Remove(pos, len);
            result.Insert(pos, newVal);
        }

        return result.ToString();
    }

    private static TestCaseJoined BuildJoinedTestCase(TestCaseDto testCase, TestCaseArrangeSanitized arrange)
    {
        return new TestCaseJoined
        {
            TestCaseId = testCase.TestCaseId ?? Guid.NewGuid(),
            Call = testCase.CallArgs
                .Select(ca => arrange.VariableMappings.TryGetValue(ca.Name, out var mapped) ? mapped : ca.Name)
                .ToArray(),
            CallFunc = testCase.ResolvedFunctionCall ?? "",
            Display = testCase.Display,
            DisplayRes = testCase.DisplayRes,
            Expected = arrange.VariableMappings.TryGetValue(testCase.Expected.Name, out var expectedMapping)
                ? expectedMapping
                : testCase.Expected.Name,
            IsPublic = testCase.IsPublic,
            OrderMatters = testCase.OrderMatters,
            ProblemProblemId = Guid.Empty,
            Setup = arrange.ArrangeStripped,
            VariableCount = arrange.VariableMappings.Count
        };
    }
}