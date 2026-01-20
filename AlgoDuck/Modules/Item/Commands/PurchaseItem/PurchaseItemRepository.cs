using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;
// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable RedundantTypeArgumentsOfMethod

namespace AlgoDuck.Modules.Item.Commands.PurchaseItem;

public interface IPurchaseItemRepository
{
    public Task<Result<PurchaseResultDto, ErrorObject<string>>> PurchaseItemAsync(PurchaseRequestDto purchaseRequest, CancellationToken cancellationToken = default);
    public Task<Result<Models.Item, ErrorObject<string>>> CheckIfItemExist(PurchaseRequestDto purchaseRequest, CancellationToken cancellationToken = default);
}

public class PurchaseItemRepository(
    ApplicationCommandDbContext dbContext
    ) : IPurchaseItemRepository
{
    
    public async Task<Result<Models.Item, ErrorObject<string>>> CheckIfItemExist(PurchaseRequestDto purchaseRequest, CancellationToken cancellationToken = default)
    {
        var requestItem = await dbContext.Items
            .AsNoTracking().FirstOrDefaultAsync(i => i.ItemId == purchaseRequest.ItemId,
                cancellationToken);

        if (requestItem == null)
            return Result<Models.Item, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"item: {purchaseRequest.ItemId} not found"));
        
        return Result<Models.Item, ErrorObject<string>>.Ok(requestItem);
    }
    
    public async Task<Result<PurchaseResultDto, ErrorObject<string>>> PurchaseItemAsync(PurchaseRequestDto purchaseRequest, CancellationToken cancellationToken = default)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        var item = await dbContext.Items.AsNoTracking()
            .FirstAsync(i => i.ItemId == purchaseRequest.ItemId, cancellationToken: cancellationToken);
        
        return await strategy.ExecuteAsync<Result<PurchaseResultDto, ErrorObject<string>>>(async () =>
        {
            await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            
            var userWithPurchases = await dbContext.ApplicationUsers
                .Include(u => u.Purchases)
                .FirstAsync(u => u.Id == purchaseRequest.UserId,
                    cancellationToken: cancellationToken);
            
            if (userWithPurchases.Purchases.Any(p => p.ItemId == purchaseRequest.ItemId))
                return Result<PurchaseResultDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest($"item: {purchaseRequest.ItemId} already owned"));

            if (userWithPurchases.Coins < item.Price)
                return Result<PurchaseResultDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest($"not enough currency to purchase item: {purchaseRequest.ItemId}"));

            userWithPurchases.Coins -= item.Price;
            userWithPurchases.Purchases.Add(item switch
            {
                DuckItem => new DuckOwnership
                {
                    ItemId = purchaseRequest.ItemId,
                    UserId = purchaseRequest.UserId,
                },
                PlantItem => new PlantOwnership
                {
                    ItemId = purchaseRequest.ItemId,
                    UserId = purchaseRequest.UserId
                },
                _ => throw new ArgumentOutOfRangeException()
            });
            
            await dbContext.SaveChangesAsync(cancellationToken);  
            await tx.CommitAsync(cancellationToken); 
            return Result<PurchaseResultDto, ErrorObject<string>>.Ok(new PurchaseResultDto
            {
                ItemId = item.ItemId,
            });
        });
    }
}
