using System.Security.Claims;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OwnedUsedItemsController(
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


public class OwnedItemsDto
{
    public required ICollection<OwnedDuckItemDto> Ducks { get; set; }
    public required ICollection<OwnedPlantItemDto> Plants { get; set; }
}

public class OwnedDuckItemDto
{
    public required Guid ItemId { get; set; }
    public required bool IsSelectedAsAvatar { get; set; }
    public required bool IsSelectedForPond { get; set; }
}

public class OwnedPlantItemDto
{
    public required Guid ItemId { get; set; }
    public required byte GridX { get; set; }
    public required byte GridY { get; set; }
    public required byte Width { get; set; }
    public required byte Height { get; set; }
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