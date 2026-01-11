using System.Text.Json.Serialization;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Queries.AdminGetFullItemDetails;


[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin/item/detail")]
public class FullItemDetailsController : ControllerBase
{
    private readonly IFullItemDetailsService _fullItemDetailsService;

    public FullItemDetailsController(IFullItemDetailsService fullItemDetailsService)
    {
        _fullItemDetailsService = fullItemDetailsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFullItemDetailsAsync([FromQuery] Guid itemId,
        CancellationToken cancellationToken)
    {
        return await _fullItemDetailsService
            .GetFullItemDetailsAsync(itemId, cancellationToken)
            .ToActionResultAsync();
    }
}
