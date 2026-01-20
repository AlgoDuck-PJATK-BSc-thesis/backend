using System.Text;
using AlgoDuck.DAL;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Modules.Problem.Shared.Repositories;
using AlgoDuck.Modules.Problem.Shared.Types;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstAnalyzer;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using AlgoDuck.Shared.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.InsertTestCaseIntoUserCode;

[ApiController]
[Route("api/problem/test-case")]
public class TestCaseInsertController : ControllerBase
{
    private readonly IInsertService _insertService;

    public TestCaseInsertController(IInsertService insertService)
    {
        _insertService = insertService;
    }

    [HttpPost]
    public async Task<IActionResult> InsertTestCaseIntoUserCodeAsync([FromBody] InsertRequestDto insertRequest,
        CancellationToken cancellationToken)
    {
        return  await _insertService.InsertTestCaseAsync(insertRequest, cancellationToken).ToActionResultAsync();
    }
}


public class InsertRequestDto
{
    public required string UserCodeB64 { get; set; }
    public required Guid ExerciseId { get; set; }
    public required Guid TestCaseId { get; set; }
}

public class InsertResultDto
{
    public required string ModifiedCodeB64 { get; set; }
}