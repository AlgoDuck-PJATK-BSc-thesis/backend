using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Status = AlgoDuck.Shared.Http.Status;

namespace AlgoDuck.Modules.Problem.Commands.CreateProblem;

[Authorize]
[ApiController]
[Route("[controller]")]
public class CreateProblemController(
    ICreateProblemService createProblemService
    ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateProblemAsync([FromBody] CreateProblemDto createProblemDto,
        CancellationToken cancellation)
    {
        var result = await createProblemService.CreateProblemAsync(createProblemDto, cancellation);
        return result.ToActionResult();
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum CreateProblemDifficulty
{
    Easy, 
    Normal, 
    Hard
}

public class ProblemTagDto
{
    public required string TagName { get; set; }
}

public class CreateProblemDto
{
    public required string TemplateB64 { get; set; }
    public required string ProblemTitle { get; set; }
    public required string ProblemDescription { get; set; }
    public required CreateProblemDifficulty ProblemDifficulty { get; set; }
    public List<ProblemTagDto> Tags { get; set; } = [];
    public List<TestCaseDto> TestCases { get; set; } = [];
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

public class TestCaseDto
{
    public required string Display { get; set; }
    public required string DisplayRes {get; set; }
    public required string ArrangeB64 { get; set; }
    public required MethodRecommendation CallMethod { get; set; }
    public List<FunctionParam> CallArgs { get; set; } = [];
    public required FunctionParam Expected { get; set; }
    public required bool IsPublic { get; set; }
    
}

public class CreateProblemResultDto
{
    
}

