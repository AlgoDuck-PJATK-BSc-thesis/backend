using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Commands.EmplacePlantOnHomePage;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmplacePlantController(
    IEmplacePlantService emplacePlantService
    ) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> EmplacePlantAsync([FromBody] EmplacePlantDto emplacePlantDto, CancellationToken cancellationToken)
    {
        var userIdRes = User.GetUserId();
        if (userIdRes.IsErr)
            return userIdRes.ToActionResult();

        emplacePlantDto.UserId = userIdRes.AsT0;
        
        var emplaceRes = await emplacePlantService.EmplacePlantAsync(emplacePlantDto, cancellationToken);
        return emplaceRes.ToActionResult();
    }
}

public class EmplacePlantDto
{
    public required Guid ItemId { get; set; }
    public required byte GridX { get; set; }
    public required byte GridY { get; set; }
    internal Guid UserId { get; set; }
}

public class EmplacePlantResult
{
    public required Guid ItemId { get; set; }
}