using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Queries.GetAllItemPages;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AllItemsPagedController(IAllItemsPagedService allItemsPagedService) :  ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllItemsPagedAsync(
        [FromQuery] int currentPage,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        return await User.GetUserId()
            .BindAsync(async userId => await allItemsPagedService.GetAllItemsPagedAsync(new PagedRequestWithAttribution
            {
                UserId = userId,
                CurrPage = currentPage,
                PageSize = pageSize,
            }, cancellationToken))
            .ToActionResultAsync();
    }
}


public class ItemDto
{
    public required Guid Id { get; set; }
    public required string ItemName { get; set; }
    public required DateTime CreatedOn { get; set; }
    public required Guid CreatedBy { get; set; }
}