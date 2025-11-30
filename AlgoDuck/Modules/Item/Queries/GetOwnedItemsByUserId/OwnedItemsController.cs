using System.Security.Claims;
using AlgoDuck.DAL;
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
    IOwnedItemsService ownedItemsService,
    ILogger<OwnedItemsController> logger
        ) : ControllerBase
{
    public async Task<IActionResult> GetOwnedItemsByUserIdAsync(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        return Ok(new StandardApiResponse<ICollection<ItemDto>>
        {
            Body = await ownedItemsService.GetOwnedItemsByUserIdAsync(userId, cancellationToken)
        });
    }
}

public interface IOwnedItemsService
{
    public Task<ICollection<ItemDto>> GetOwnedItemsByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}

public class OwnedItemsService(
    IOwnedItemsRepository ownedItemsRepository
    ) : IOwnedItemsService
{
    public async Task<ICollection<ItemDto>> GetOwnedItemsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await ownedItemsRepository.GetOwnedItemsByUserId(userId, cancellationToken);
    }
}

public interface IOwnedItemsRepository
{
    public Task<ICollection<ItemDto>> GetOwnedItemsByUserId(Guid userId, CancellationToken cancellationToken);
}

public class OwnedItemsRepository(
    ApplicationQueryDbContext dbContext
    ) : IOwnedItemsRepository
{
    public async Task<ICollection<ItemDto>> GetOwnedItemsByUserId(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Items
            .Include(e => e.Purchases)
            .Where(e => e.Purchases.Select(p => p.UserId).Contains(userId))
            .Select(i => new ItemDto
            {
                ItemId = i.ItemId
            }).ToListAsync(cancellationToken: cancellationToken); 
    }
}

public class ItemDto
{
    public Guid ItemId { get; set; }
}

public static class ClaimsPrincipalExtension
{
    public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var findFirstValue = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                             throw new UserNotFoundException("User id not found");
        try
        {
            return Guid.Parse(findFirstValue);
        }
        catch (FormatException e)
        {
            throw new UserNotFoundException($"Invalid user id {findFirstValue}");
        }
    }
}