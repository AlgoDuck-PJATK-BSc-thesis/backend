using System.ComponentModel.DataAnnotations;
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
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.InsertTestCaseIntoUserCode;

[ApiController]
[Route("api/problem/test-case")]
public class TestCaseInsertController : ControllerBase
{
    private readonly IInsertService _insertService;
    private readonly IValidator<InsertRequestDto> _validator;

    public TestCaseInsertController(IInsertService insertService, IValidator<InsertRequestDto> validator)
    {
        _insertService = insertService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> InsertTestCaseIntoUserCodeAsync([FromBody] InsertRequestDto insertRequest,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(insertRequest, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        return  await _insertService.InsertTestCaseAsync(insertRequest, cancellationToken).ToActionResultAsync();
    }
}


public class InsertRequestDto
{
    private const int MaxCodeLengthBytes = 128 * 1024;

    [Display(Name = "Code")]
    [MaxLength(MaxCodeLengthBytes, ErrorMessage = "{0} length cannot exceed {1} bytes")]
    public required string UserCodeB64 { get; set; }
    public required Guid ExerciseId { get; set; }
    public required Guid TestCaseId { get; set; }
}

public class InsertResultDto
{
    public required string ModifiedCodeB64 { get; set; }
}