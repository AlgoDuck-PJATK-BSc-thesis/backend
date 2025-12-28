using System.Security.Claims;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Extensions;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OwnedItemsController(
    IOwnedItemsService ownedItemsService
    ) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetOwnedItemsByUserIdAsync(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId.IsErr)
            return userId.ToActionResult();

        var ownedItemsResult = await ownedItemsService.GetOwnedItemsByUserIdAsync(userId.AsT0, cancellationToken);
        return ownedItemsResult.ToActionResult();
    }
}


public class ItemDto
{
    public required Guid ItemId { get; set; }
    public required ItemType ItemType { get; set; }
}


/*TODO: Move this somewhere*/
public static class ClaimsPrincipalExtension
{
    public static Result<Guid, ErrorObject<string>> GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var findFirstValue = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(findFirstValue))
        {
            return Result<Guid, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest("User Id not found"));
        }
        try
        {
            return Result<Guid, ErrorObject<string>>.Ok(Guid.Parse(findFirstValue));
        }
        catch (FormatException)
        {
            return Result<Guid, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest("User Id is not a valid GUID"));
        }
    }
}