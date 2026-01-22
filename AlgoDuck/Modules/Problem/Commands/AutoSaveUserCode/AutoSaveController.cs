using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable ConvertClosureToMethodGroup

namespace AlgoDuck.Modules.Problem.Commands.AutoSaveUserCode;

[ApiController]
[Route("api/[controller]")]
public class AutoSaveController : ControllerBase
{
    private readonly IAutoSaveService _autoSaveService;

    private readonly IValidator<AutoSaveDto> _validator;
    public AutoSaveController(IAutoSaveService autoSaveService, IValidator<AutoSaveDto> validator)
    {
        _autoSaveService = autoSaveService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> AutoSaveCodeAsync([FromBody] AutoSaveDto autoSaveDto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(autoSaveDto,  cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        return await User
            .GetUserId()
            .BindAsync(async idResult =>
            {
                autoSaveDto.UserId = idResult;
                return await _autoSaveService.AutoSaveCodeAsync(autoSaveDto, cancellationToken);
            }).ToActionResultAsync("autosave completed successfully");
    }
}

public class AutoSaveResultDto
{
    public required Guid ProblemId { get; set; } 
}