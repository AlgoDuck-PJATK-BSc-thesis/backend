using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.AdminGetFullItemDetails;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetFullItemDetails;

public interface IFullItemDetailsRepository
{
    public Task<Result<FullItemDetailsDto, ErrorObject<string>>> GetFullItemDetailsAsync(Guid itemId, CancellationToken cancellationToken = default);
    
}

public class FullItemDetailsRepository(
    ApplicationQueryDbContext dbContext
    ) : IFullItemDetailsRepository
{
    public async Task<Result<FullItemDetailsDto, ErrorObject<string>>> GetFullItemDetailsAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        var itemGeneral = await dbContext.Items
            .Where(i => i.ItemId == itemId)
            .Select(e => new FullItemDetailsDto
            {
                ItemId = e.ItemId,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = Guid.NewGuid(),
                ItemType = EF.Property<string>(e, "type"),
                ItemDescription = e.Description,
                ItemName = e.Name,
                Purchases = e.Purchases.Count,
                Price = e.Price,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (itemGeneral == null)
            return Result<FullItemDetailsDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"item: {itemId} not found"));
        
        var itemDetails = await GetTypedObjectDetails(itemGeneral.ItemType, itemId, cancellationToken);
        
        if (itemDetails.IsErr)
            return Result<FullItemDetailsDto, ErrorObject<string>>.Err(itemDetails.AsErr!);

        itemGeneral.ItemTypeSpecificData = itemDetails.AsOk!;
        
        return Result<FullItemDetailsDto, ErrorObject<string>>.Ok(itemGeneral);
    }

    private async Task<Result<IItemTypeSpecificData, ErrorObject<string>>> GetTypedObjectDetails(string type, Guid itemId,
        CancellationToken cancellationToken = default)
    {
        IItemTypeSpecificData? result = type switch
        {
            "Duck" => await dbContext.DuckItems.Where(d => d.ItemId == itemId).Select(d => new DuckData()).FirstOrDefaultAsync(cancellationToken: cancellationToken),
            "Plant" => await dbContext.PlantItems.Where(d => d.ItemId == itemId).Select(d => new PlantData()
            {
                Height = d.Height,
                Width = d.Width
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken),
            _ => null
        };
        if (result == null)
            return Result<IItemTypeSpecificData, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"item: {itemId} not found"));
        return Result<IItemTypeSpecificData, ErrorObject<string>>.Ok(result);
    }
}