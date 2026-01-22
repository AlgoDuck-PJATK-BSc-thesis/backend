using System.ComponentModel.DataAnnotations;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.UpdateChatName;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UpdateChatNameController : ControllerBase
{
    private readonly IUpdateChatNameService _updateChatNameService;
    private readonly IValidator<UpdateChatNameDto> _validator;

    public UpdateChatNameController(IUpdateChatNameService updateChatNameService, IValidator<UpdateChatNameDto> validator)
    {
        _updateChatNameService = updateChatNameService;
        _validator = validator;
    }

    [HttpPut]
    public async Task<IActionResult> UpdateChatName([FromBody] UpdateChatNameDto updateChatNameDto, CancellationToken cancellationToken)
    {
        var validationResult =  await _validator.ValidateAsync(updateChatNameDto, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        return await User.GetUserId().BindAsync(async userId =>
        {
            updateChatNameDto.UserId = userId;
            return await _updateChatNameService.UpdateChatName(updateChatNameDto, cancellationToken);
        }).ToActionResultAsync();
    }
}

public class UpdateChatNameDto{
    [MaxLength(128, ErrorMessage = "Chat name cannot exceed 128 characters.")]
    [MinLength(3, ErrorMessage = "Chat name must be at least 3 characters.")]
    public required string NewChatName { get; init; }
    public required Guid ChatId { get; init; }
    internal Guid UserId { get; set; }
}

public class UpdateChatNameResult
{
    public required string NewChatName { get; set; }
}