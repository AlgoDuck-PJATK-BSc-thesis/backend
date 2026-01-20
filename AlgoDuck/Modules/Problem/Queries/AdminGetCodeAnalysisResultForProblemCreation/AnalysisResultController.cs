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
public class AnalysisResultController : ControllerBase
{
    private readonly IAnalysisResultService _analysisResultService;

    public AnalysisResultController(IAnalysisResultService analysisResultService)
    {
        this._analysisResultService = analysisResultService;
    }

    [HttpPost]
    public async Task<IActionResult> GetAnalysisResult([FromBody] AnalysisRequestDto request)
    {
        return await _analysisResultService
            .GetAnalysisResult(request)
            .ToActionResultAsync();
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