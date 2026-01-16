using System.Text;
using System.Text.Json;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;
using AlgoDuck.Modules.Problem.Shared.Repositories;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.LoadProblemCreateFormStateFromProblem;

public interface IFormStateLoadService
{
    public Task<Result<UpsertProblemDto, ErrorObject<string>>> LoadFormStateAsync(Guid problemId, CancellationToken cancellationToken = default);
}

public class FormStateLoadService(
    ISharedProblemRepository problemRepository,
    IFormStateLoadRepository formStateLoadRepository
    ) : IFormStateLoadService
{
    public async Task<Result<UpsertProblemDto, ErrorObject<string>>> LoadFormStateAsync(Guid problemId, CancellationToken cancellationToken = default)
    {
        var problemTemplateResult = await problemRepository.GetTemplateAsync(problemId, cancellationToken);
        if (problemTemplateResult.IsErr)
            return Result<UpsertProblemDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"template for problem {problemId} not found"));
        var problemTemplate = problemTemplateResult.AsOk!;
        
        var problemInfosResult = await problemRepository.GetProblemInfoAsync(problemId, cancellationToken: cancellationToken);
        if (problemInfosResult.IsErr)
            return Result<UpsertProblemDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"template for problem {problemId} not found"));
        var problemInfos = problemInfosResult.AsOk!;
        
        var problemTestCasesResult = await problemRepository.GetTestCasesAsync(problemId, cancellationToken);
        if (problemTestCasesResult.IsErr)
            return Result<UpsertProblemDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"template for problem {problemId} not found"));
        var problemTestCases = problemTestCasesResult.AsOk!;

        
        var problemDataFullResult = await formStateLoadRepository.GetFullProblemDataAsync(problemId, cancellationToken);
        if (problemDataFullResult.IsErr)
            return Result<UpsertProblemDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"template for problem {problemId} not found"));
        var problemDataFull = problemDataFullResult.AsOk!;
        Console.WriteLine(JsonSerializer.Serialize(problemTestCases));

        
        /*
         * Technically the chances of this being thrown are basically 0,
         * since atp we can guarantee that the template contains valid java code.
         * So it's more that I don't really fully trust my implementation of the parse
         * (not that it's let me down before but better safe than sorry)
         */

        Console.WriteLine(problemTemplate.Template);
        var templateBuilder = new StringBuilder(problemTemplate.Template);
        CodeAnalysisResult analysisResult;
        try
        {
            var templateAnalyzer = new AnalyzerSimple(templateBuilder);
            analysisResult = templateAnalyzer.AnalyzeUserCode(ExecutionStyle.Execution);
            if (analysisResult.MainMethodIndices == null) /* vague error since main is autoinjected so while the type technically says nullable it's not really */
                return Result<UpsertProblemDto, ErrorObject<string>>.Err(ErrorObject<string>.InternalError($"Could not parse template"));
        }
        catch (JavaSyntaxException ex) 
        {
            return Result<UpsertProblemDto, ErrorObject<string>>.Err(ErrorObject<string>.InternalError($"Could not parse template for problem {problemId}"));    
        }

        List<TestCaseDto> testCases = [];
        problemTestCases.Select((testCase, index) => (testCase, index)).ToList().ForEach(tuple =>
        {
            var (testCase, index) = tuple;
            var buildTestCaseDtoResult =
                ResolveTestCaseDtoParts(testCase, index, analysisResult, templateBuilder.ToString());
            if (buildTestCaseDtoResult.IsOk)
                testCases.Add(buildTestCaseDtoResult.AsOk!);
        });
        
        return Result<UpsertProblemDto, ErrorObject<string>>.Ok(new UpsertProblemDto
        {
            CategoryId = problemDataFull.CategoryId,
            DifficultyId = problemDataFull.DifficultyId,
            ProblemDescription = problemInfos.Description,
            ProblemTitle = problemInfos.Title,
            TemplateB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(problemTemplate.Template)),
            Tags = problemDataFull.Tags.Select(t => new ProblemTagDto()
            {
                TagName = t.TagName
            }).ToList(),
            TestCases = testCases
        });
    }

    private Result<TestCaseDto, ErrorObject<string>> BuildTestCaseDto(TestCaseJoined testCase, int testCaseIndex, KeyValuePair<AstNodeMemberFunc<AstNodeClass>, string>? callFunc, ICollection<AstNodeScopeMemberVar> callArgs, AstNodeScopeMemberVar? expected)
    {
        if (callFunc == null)
            return Result<TestCaseDto, ErrorObject<string>>.Err(
                ErrorObject<string>.BadRequest("call function is null"));
        
        if (expected == null)
            return Result<TestCaseDto, ErrorObject<string>>.Err(
                ErrorObject<string>.BadRequest("expected value is null"));


        return Result<TestCaseDto, ErrorObject<string>>.Ok(new TestCaseDto
        {
            TestCaseId = testCase.TestCaseId,
            ArrangeB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(GetArrangeWrapped(testCase.Setup, testCaseIndex))),
            CallArgs = callArgs.Select(ca => new FunctionParam()
            {
                Name = ca.Identifier?.Value ?? "<undefined>",
                Type = ca.Type.Match(t1 => t1.ToString(), t2 => t2.ToString(), t3 => t3.ToString())
            }).ToList(),
            CallMethod = new MethodRecommendation
            {
                AccessModifier = callFunc.Value.Key.AccessModifier.ToString(),
                FunctionParams = callFunc.Value.Key.FuncArgs.Select(fa => new FunctionParam()
                {
                    Name = fa.Identifier?.Value ?? "<undefined>",
                    Type = fa.Type.Match(t1 => t1.ToString(), t2 => t2.ToString(), t3 => t3.ToString())
                }).ToList(),
                Generics = callFunc.Value.Key.GenericTypes.Select(g => g.ToString()).ToList(),
                MethodName = callFunc.Value.Key.Identifier?.Value ?? "<undefined>",
                Modifiers = callFunc.Value.Key.Modifiers.Select(m => m.ToString()).ToList(),
                QualifiedName = callFunc.Value.Value,
                ReturnType = callFunc.Value.Key.FuncReturnType?.Match(
                    t1 => t1.ToString(), t2 => t2.ToString(),
                    t3 => t3.ToString(), t4 => t4.ToString()) ?? "<undefined>",
            },
            Display = testCase.Display,
            DisplayRes = testCase.DisplayRes,
            IsPublic = testCase.IsPublic,
            OrderMatters = testCase.OrderMatters,
            Expected = new FunctionParam
            {
                Name = expected.Identifier?.Value ?? "<undefined>",
                Type = expected.Type.Match(t1 => t1.ToString(), t2 => t2.ToString(), t3 => t3.ToString())
            },

        });
    }

    private Result<TestCaseDto, ErrorObject<string>> ResolveTestCaseDtoParts(TestCaseJoined testCase, int index, CodeAnalysisResult analysisResult, string template)
    {
        if (analysisResult.MainMethodIndices == null)
            return Result<TestCaseDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest("no main method found"));
        
        InterpolateTestCaseEntrypointClassname(testCase, analysisResult);
        InterpolateTestCasePlaceholders(testCase, index);

        Console.WriteLine(JsonSerializer.Serialize(testCase));
        var testCaseBuilder = new StringBuilder(template);
        testCaseBuilder.Insert(analysisResult.MainMethodIndices.MethodFileEndIndex, testCase.Setup);

        Console.WriteLine(testCaseBuilder);
        var analyzer = new AnalyzerSimple(testCaseBuilder);
        var tcAnalysisResult = analyzer.AnalyzeUserCode(ExecutionStyle.Execution);

        if (tcAnalysisResult.Main.FuncScope == null) 
            return Result<TestCaseDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest("no main scope found"));
        
        var callMethodWithQualifiedPath = ResolveCallMethodWithQualifiedName(analyzer, tcAnalysisResult, testCase);
        
        List<AstNodeScopeMemberVar> variables = [];
        analyzer.GetAllVariablesAccessibleFromScope(tcAnalysisResult.Main.FuncScope.OwnScope, variables);
        Console.WriteLine(testCaseBuilder);
        
        var callVars = variables.Where(v => testCase.Call.Contains(v.Identifier?.Value)).ToList();
        
        var expectedVar = variables.FirstOrDefault(v => testCase.Expected == v.Identifier?.Value);

        return BuildTestCaseDto(testCase, index, callMethodWithQualifiedPath, callVars, expectedVar);
    }

    private static string GetArrangeWrapped(string content, int testCaseIndex) =>
        $"public class TestCase{testCaseIndex}{{\n\tpublic static void main(String[] args){{\n\t\t{content}\n\t}}\n}}";


    private static void InterpolateTestCaseEntrypointClassname(TestCaseJoined testCase, CodeAnalysisResult analysisResult)
    {
        testCase.Setup = testCase.Setup.Replace("${ENTRYPOINT_CLASS_NAME}", analysisResult.MainClassName);
    }
    private static void InterpolateTestCasePlaceholders(TestCaseJoined testCase, int index)
    {
        Console.WriteLine(index);
        Console.WriteLine($"Old setup: {testCase.Setup}");
        Console.WriteLine(testCase.VariableCount);
        for (var i = 0; i < testCase.VariableCount; ++i)
        {
            var interpolationString = $"{{tc_{index}_var_{i}}}";
            Console.WriteLine(interpolationString);
            testCase.Setup = testCase.Setup.Replace(interpolationString, $"var{i}");
            for (var j = 0; j < testCase.Call.Length; j++)
            {
                testCase.Call[j] = testCase.Call[j].Replace(interpolationString, $"var{i}");
            }
            testCase.Expected = testCase.Expected.Replace(interpolationString, $"var{i}");
        }
        Console.WriteLine($"New setup: {testCase.Setup}");
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
    }

    private static KeyValuePair<AstNodeMemberFunc<AstNodeClass>, string>? ResolveCallMethodWithQualifiedName(AnalyzerSimple analyzer, CodeAnalysisResult tcAnalysisResult, TestCaseJoined testCase)
    {
        if (tcAnalysisResult.Main.FuncScope == null)
            return null;
        Dictionary<AstNodeMemberFunc<AstNodeClass>, string> methods = [];
        analyzer.GetAllFunctionsAccessibleFromScope(tcAnalysisResult.Main.FuncScope.OwnScope, methods);

        return methods.FirstOrDefault(keyVal =>
        {
            var resolveResult = analyzer.RecursiveResolveFunctionCall(tcAnalysisResult.Main.FuncScope.OwnScope,
                keyVal.Value.Split("."));

            return resolveResult.IsOk && resolveResult.AsOk! == testCase.CallFunc.Replace("${ENTRYPOINT_CLASS_NAME}", tcAnalysisResult.MainClassName);
        });
    }
}