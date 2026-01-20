using System.Text;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.AdminGetCodeAnalysisResultForProblemCreation;

[Authorize(Roles = "admin")]
[Route("api/[controller]")]
public class AnalysisResultController(
    IAnalysisResultService analysisResultService
    ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> GetAnalysisResult([FromBody] AnalysisRequestDto request)
    {
        return await analysisResultService
            .GetAnalysisResult(request)
            .ToActionResultAsync();
    }
}

public interface IAnalysisResultService
{
    public Task<Result<AnalysisResultDto, ErrorObject<string>>> GetAnalysisResult(AnalysisRequestDto requestDto);
}

public class AnalysisResultService : IAnalysisResultService
{
    public async Task<Result<AnalysisResultDto, ErrorObject<string>>> GetAnalysisResult(AnalysisRequestDto requestDto)
    {
        try
        {
            var arrange = Encoding.UTF8.GetString(Convert.FromBase64String(requestDto.ArrangeB64));
            var template = Encoding.UTF8.GetString(Convert.FromBase64String(requestDto.TemplateB64));

            var templateBuilder = new StringBuilder(template);
            var arrangeBuilder = new StringBuilder(arrange);

            var analyzerTemplate = new AnalyzerSimple(templateBuilder);
            var analyzerArrange = new AnalyzerSimple(arrangeBuilder);

            var templateResult = analyzerTemplate.AnalyzeUserCode(ExecutionStyle.Execution);
            var arrangeResult = analyzerArrange.AnalyzeUserCode(ExecutionStyle.Execution);

            var arrangeMainMethodLength = arrangeResult.MainMethodIndices!.MethodFileEndIndex -
                                          arrangeResult.MainMethodIndices!.MethodFileBeginIndex;
            templateBuilder.Insert(templateResult.MainMethodIndices!.MethodFileBeginIndex + 1,
                arrange.AsSpan(arrangeResult.MainMethodIndices!.MethodFileBeginIndex + 1, arrangeMainMethodLength - 1));

            var analyzerFull = new AnalyzerSimple(templateBuilder);

            var fullResult = analyzerFull.AnalyzeUserCode(ExecutionStyle.Execution);

            List<AstNodeScopeMemberVar> variables = [];
            Dictionary<AstNodeMemberFunc<AstNodeClass>, string> methods = [];

            analyzerFull.GetAllVariablesAccessibleFromScope(fullResult.Main.FuncScope!.OwnScope, variables);
            analyzerFull.GetAllFunctionsAccessibleFromScope(fullResult.Main.FuncScope!.OwnScope, methods);

            return Result<AnalysisResultDto, ErrorObject<string>>.Ok(new AnalysisResultDto
            {
                Methods = methods.Select(m => new MethodRecommendation
                {
                    AccessModifier = m.Key.AccessModifier.ToString(),
                    FunctionParams = m.Key.FuncArgs.Select(a => new FunctionParam
                    {
                        Name = a.Identifier?.Value ?? "<undefined>",
                        Type = a.Type.Match(
                            t1 => t1.ToString(),
                            t2 => t2.ToString(),
                            t3 => t3.ToString()),
                    }).ToList(),
                    MethodName = m.Key.Identifier?.Value ?? "<undefined>",
                    QualifiedName = m.Value,
                    Generics = m.Key.GenericTypes.Select(g => g.ToString()).ToList(),
                    Modifiers = m.Key.Modifiers.Select(mm => mm.ToString()).ToList(),
                    ReturnType = m.Key.FuncReturnType == null
                        ? "<undefined>"
                        : m.Key.FuncReturnType.Value.Match(
                            t1 => t1.ToString(),
                            t2 => t2.ToString(),
                            t3 => t3.ToString(),
                            t4 => t4.ToString())
                }).ToList(),
                Variables = variables.Select(v => new VariableRecommendation
                {
                    Name = v.Identifier?.Value ?? "<undefined>",
                    Type = v.Type.Match(
                        t1 => t1.ToString(),
                        t2 => t2.ToString(),
                        t3 => t3.ToString())
                }).ToList(),
            });
        }
        catch (JavaSyntaxException ex)
        {
            return  Result<AnalysisResultDto, ErrorObject<string>>.Err(ErrorObject<string>.ValidationError("syntax_error"));
        }
    }
}


public class AnalysisRequestDto
{
    public required string TemplateB64 { get; set; }
    public required string ArrangeB64 { get; set; }
}

public class AnalysisResultDto
{
    public List<MethodRecommendation> Methods { get; set; } = [];
    public List<VariableRecommendation> Variables { get; set; } = [];
}


public class MethodRecommendation
{
    public required string MethodName { get; set; }
    public required string QualifiedName { get; set; }
    public List<FunctionParam> FunctionParams { get; set; } = [];
    public List<string> Generics { get; set; } = [];
    public List<string> Modifiers { get; set; } = [];
    public required string AccessModifier { get; set; }
    public required string ReturnType { get; set; }
}


public class FunctionParam
{
    public required string Type { get; set; }
    public required string Name { get; set; }
}

public class VariableRecommendation
{
    public required string Type { get; set; }
    public required string Name { get; set; }
}