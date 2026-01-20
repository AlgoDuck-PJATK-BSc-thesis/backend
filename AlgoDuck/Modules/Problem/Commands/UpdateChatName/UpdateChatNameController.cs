using System.ComponentModel.DataAnnotations;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.UpdateChatName;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UpdateChatNameController : ControllerBase
{
    private readonly IUpdateChatNameService _updateChatNameService;

    public UpdateChatNameController(IUpdateChatNameService updateChatNameService)
    {
        _updateChatNameService = updateChatNameService;
    }

    [HttpPut]
    public async Task<IActionResult> UpdateChatName([FromBody] UpdateChatNameDto updateChatNameDto, CancellationToken cancellationToken)
    {
        return await User.GetUserId().BindAsync(async userId =>
        {
            updateChatNameDto.UserId = userId;
            return await _updateChatNameService.UpdateChatName(updateChatNameDto, cancellationToken);
        }).ToActionResultAsync();
    }
}

public class UpdateChatNameDto{
    [MaxLength(128)]
    [MinLength(3)]
    public required string NewChatName { get; init; }
    public required Guid ChatId { get; init; }
    internal Guid UserId { get; set; }
}

public class UpdateChatNameResult
{
    public required string NewChatName { get; set; }
}