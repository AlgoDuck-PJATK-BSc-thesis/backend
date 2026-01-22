using System.ComponentModel.DataAnnotations;
using AlgoDuck.Models;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.CreateEditorLayout;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CreateLayoutController : ControllerBase
{
    private readonly ICreateLayoutService _createLayoutService;
    private readonly IValidator<LayoutCreateDto> _validator;

    public CreateLayoutController(ICreateLayoutService createLayoutService, IValidator<LayoutCreateDto> validator)
    {
        _createLayoutService = createLayoutService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLayoutAsync([FromBody] LayoutCreateDto createDto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(createDto,  cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        return await User.GetUserId().BindAsync(async userId =>
        {
            createDto.UserId = userId;
            return await _createLayoutService.CreateLayoutAsync(createDto, cancellationToken);
        }).ToActionResultAsync();
    }
}

public class LayoutCreateDto
{
    [Display(Name = "Layout configuration")]
    [MaxLength(10240, ErrorMessage = "{0} object too large, cannot exceed {1} bytes")]
    public required string LayoutContent { get; set; }
    [Display(Name = "Layout name")]
    [MaxLength(128, ErrorMessage = "{0} cannot exceed {1} characters")]
    public required string LayoutName { get; set; }
    internal Guid UserId { get; set; }
}

public class LayoutCreateResultDto
{
    public required Guid LayoutId { get; set; } 
}