using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Status = AlgoDuck.Shared.Http.Status;

namespace AlgoDuck.Modules.Item.Commands.PurchaseItem;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class PurchaseController(
    IPurchaseItemService purchaseItemService
    ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PurchaseItemAsync(
        [FromBody] PurchaseRequestDto purchaseRequest,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId.IsErr)
            return userId.ToActionResult();
        
        return Ok(new StandardApiResponse<PurchaseResultDto>
        {
            Body = await purchaseItemService.PurchaseItemAsync(new PurchaseRequestInternalDto
            {
                PurchaseRequestDto = purchaseRequest,
                RequestingUserId = userId.AsT0,
            }, cancellationToken)
        });
    }
    
}

public interface IPurchaseItemService
{
    public Task<PurchaseResultDto> PurchaseItemAsync(PurchaseRequestInternalDto purchaseRequest, CancellationToken cancellationToken);
}

public class PurchaseItemService(
    IPurchaseItemRepository purchaseItemRepository
    ) : IPurchaseItemService
{
    public async Task<PurchaseResultDto> PurchaseItemAsync(PurchaseRequestInternalDto purchaseRequest, CancellationToken cancellationToken)
    {
        return await purchaseItemRepository.PurchaseItemAsync(purchaseRequest, cancellationToken);
    }
}

public interface IPurchaseItemRepository
{
    public Task<PurchaseResultDto> PurchaseItemAsync(PurchaseRequestInternalDto purchaseRequest, CancellationToken cancellationToken);
}

public class PurchaseItemRepository(
    ApplicationCommandDbContext dbContext
    ) : IPurchaseItemRepository
{
    public async Task<PurchaseResultDto> PurchaseItemAsync(PurchaseRequestInternalDto purchaseRequest, CancellationToken cancellationToken)
    {
        var requestItem = await dbContext.Items
            .AsNoTracking().
            FirstOrDefaultAsync(i => i.ItemId == purchaseRequest.PurchaseRequestDto.ItemId, cancellationToken)
                ?? throw new ItemNotFoundException();
        
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            
            var userWithPurchases = await dbContext.ApplicationUsers
                                        .Include(u => u.Purchases)
                                        .FirstOrDefaultAsync(u => u.Id == purchaseRequest.RequestingUserId,
                                            cancellationToken: cancellationToken)
                                    ?? throw new UserNotFoundException();
            
            if (userWithPurchases.Purchases.Any(p => p.ItemId == purchaseRequest.PurchaseRequestDto.ItemId))
                throw new ItemAlreadyPurchasedException();

            if (userWithPurchases.Coins < requestItem.Price)
                throw new NotEnoughCurrencyException();

            userWithPurchases.Coins -= requestItem.Price;
            userWithPurchases.Purchases.Add(new Purchase
            {
                ItemId = requestItem.ItemId,
                UserId = userWithPurchases.Id
            });
            
            await dbContext.SaveChangesAsync(cancellationToken);  
            await tx.CommitAsync(cancellationToken); 
        });

        return new PurchaseResultDto
        {
            ItemId = requestItem.ItemId,
        };
    }
}

public class ItemNotFoundException(string? msg = "") : Exception(msg);
public class ItemAlreadyPurchasedException(string? msg = "") : Exception(msg);
public class NotEnoughCurrencyException(string? msg = "") : Exception(msg);


public class PurchaseResultDto
{
    public required Guid ItemId { get; set; }
}

public class PurchaseRequestDto
{
    public required Guid ItemId { get; set; }
}

public class PurchaseRequestInternalDto
{
    public required Guid RequestingUserId { get; set; }
    public required PurchaseRequestDto PurchaseRequestDto { get; set; }
}