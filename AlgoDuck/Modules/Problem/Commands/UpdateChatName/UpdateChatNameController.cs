using System.ComponentModel.DataAnnotations;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.UpdateChatName;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UpdateChatNameController(
    IUpdateChatNameService updateChatNameService
) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> UpdateChatName([FromBody] UpdateChatNameDto updateChatNameDto, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        
        if (userId.IsErr)
            return userId.ToActionResult();

        updateChatNameDto.UserId = userId.AsT0;
            
        var res = await updateChatNameService.UpdateChatName(updateChatNameDto, cancellationToken);
        return res.ToActionResult();
    }
}

public class UpdateChatNameDto{
    [MaxLength(128)]
    [MinLength(3)]
    public required string NewChatName { get; set; }
    public required Guid ChatId { get; set; }
    internal Guid UserId { get; set; }
}

public class UpdateChatNameResult
{
    public required string NewChatName { get; set; }
}