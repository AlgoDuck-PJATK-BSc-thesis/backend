using System.Text.Json;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Item.Commands.UpsertItem.CreateItem;
using AlgoDuck.Modules.Item.Queries.AdminGetFullItemDetails.Types;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Item.Queries.AdminGetFullItemDetails;

public interface IFullItemDetailsRepository
{
    public Task<Result<ItemDetailsCore, ErrorObject<string>>> GetItemDetailsCore(Guid itemId, CancellationToken cancellationToken = default);

    public Task<Result<IItemTypeSpecificData, ErrorObject<string>>> GetTypedObjectDetails(string type,
        Guid itemId,
        CancellationToken cancellationToken = default);
    
    public Task<Result<ItemSpecificStatistics, ErrorObject<string>>> GetItemStatisticsAsync(Guid itemId,
        string itemType, CancellationToken cancellationToken = default);

    public Task<Result<ItemPurchaseTimeseriesData, ErrorObject<string>>> GetItemPurchaseTimeseriesDataAsync(TimeSeriesDataRequest request,
        CancellationToken cancellationToken = default);

}

public class FullItemDetailsRepository : IFullItemDetailsRepository
{
    private readonly ApplicationQueryDbContext _dbContext;

    public FullItemDetailsRepository(ApplicationQueryDbContext dbContext, IOptions<SpriteLegalFileNamesConfiguration> itemSprites)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IItemTypeSpecificData, ErrorObject<string>>> GetTypedObjectDetails(string type, Guid itemId,
        CancellationToken cancellationToken = default)
    {
        IItemTypeSpecificData? result = type switch
        {
            "Duck" => await _dbContext.DuckItems.Where(d => d.ItemId == itemId).Select(d => new DuckData()).FirstOrDefaultAsync(cancellationToken: cancellationToken),
            "Plant" => await _dbContext.PlantItems.Where(d => d.ItemId == itemId).Select(d => new PlantData
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

    public async Task<Result<ItemDetailsCore, ErrorObject<string>>> GetItemDetailsCore(Guid itemId, CancellationToken cancellationToken = default)
    {
        var itemGeneral = await _dbContext.Items
            .Where(i => i.ItemId == itemId)
            .Select(e => new ItemDetailsCore
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
            return Result<ItemDetailsCore, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"item: {itemId} not found"));
        
        
        return Result<ItemDetailsCore, ErrorObject<string>>.Ok(itemGeneral);
    }

    public async Task<Result<ItemSpecificStatistics, ErrorObject<string>>> GetItemStatisticsAsync(Guid itemId, string itemType, CancellationToken cancellationToken = default)
    {
        switch (itemType)
        {
            case "Plant":
            {
                var plantResult = await GetPlantOwnershipStatistics(itemId, cancellationToken);
                if (plantResult.IsErr)
                    return Result<ItemSpecificStatistics, ErrorObject<string>>.Err(plantResult.AsErr!);
                return Result<ItemSpecificStatistics, ErrorObject<string>>.Ok(plantResult.AsOk!);
            }
            case "Duck":
            {
                var plantResult = await GetDuckOwnershipStatistics(itemId, cancellationToken);
                if (plantResult.IsErr)
                    return Result<ItemSpecificStatistics, ErrorObject<string>>.Err(plantResult.AsErr!);
                return Result<ItemSpecificStatistics, ErrorObject<string>>.Ok(plantResult.AsOk!);
            }   
            default:
                return Result<ItemSpecificStatistics, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest("${itemType} not supported}"));
        }
    }

    public async Task<Result<ItemPurchaseTimeseriesData, ErrorObject<string>>> GetItemPurchaseTimeseriesDataAsync(
        TimeSeriesDataRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Purchases.Where(p => p.PurchasedAt > request.StartDate);

        var buckets = request.Granularity switch
        {
            TimeseriesGranularity.Month => await query
                .GroupBy(p => new { p.PurchasedAt.Year, p.PurchasedAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new TimeseriesBucket
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:D2}", 
                    Value = g.Count()
                }).ToListAsync(cancellationToken),

            TimeseriesGranularity.Day => await query
                .GroupBy(p => p.PurchasedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new TimeseriesBucket
                {
                    Label = g.Key.ToString("yyyy-MM-dd"),
                    Value = g.Count()
                }).ToListAsync(cancellationToken),

            _ => throw new ArgumentOutOfRangeException()
        };

        return Result<ItemPurchaseTimeseriesData, ErrorObject<string>>.Ok(
            new ItemPurchaseTimeseriesData
            {
                StartDate = request.StartDate,
                Granularity = request.Granularity,
                Buckets = buckets
            });
    }


    private async Task<Result<DuckOwnershipStatistics, ErrorObject<string>>> GetDuckOwnershipStatistics(Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var allDuckOwnerships = await _dbContext.DuckOwnerships.Where(d => d.ItemId == itemId)
            .ToListAsync(cancellationToken: cancellationToken);
        if (allDuckOwnerships.Count == 0)
            return Result<DuckOwnershipStatistics, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"No data for item: {itemId} found"));

        var selectedForPondCount = allDuckOwnerships.Count(d => d.SelectedForPond);
        var selectedForAvatarCount = allDuckOwnerships.Count(d => d.SelectedAsAvatar);
        var usedByCount = allDuckOwnerships.Select(so => new { so.UserId, so.SelectedAsAvatar })
            .Concat(allDuckOwnerships.Select(so => new { so.UserId, so.SelectedAsAvatar })).Select(s => s.UserId)
            .Distinct().Count();
        
        var totalUserCount = await _dbContext.ApplicationUsers.CountAsync(cancellationToken: cancellationToken);
        return Result<DuckOwnershipStatistics, ErrorObject<string>>.Ok(new DuckOwnershipStatistics
        {
            OwnedByCount = allDuckOwnerships.Count,
            OwnedByPercentageOfPopulation = (double)allDuckOwnerships.Count / totalUserCount,
            UsedByPercentageOfPopulation = (double) usedByCount / totalUserCount, 
            UsedByCount = usedByCount,
            UsedAsAvatar = selectedForAvatarCount,
            UsedForPond = selectedForPondCount,
            UsedAsAvatarPercentageOfPopulation =  (double)selectedForAvatarCount / totalUserCount,
            UsedForPondPercentageOfPopulation =  (double)selectedForPondCount / totalUserCount,
        });
    }
    
    private async Task<Result<PlantOwnershipStatistics, ErrorObject<string>>> GetPlantOwnershipStatistics(Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var allPlantOwnerships = await _dbContext.PlantOwnerships.Where(p => p.ItemId == itemId).ToListAsync(cancellationToken: cancellationToken);

        var selectedForPond = allPlantOwnerships.Where(po => po is { GridX: not null, GridY: not null }).ToList();
        var totalUserCount = await _dbContext.ApplicationUsers.CountAsync(cancellationToken: cancellationToken);
        return Result<PlantOwnershipStatistics, ErrorObject<string>>.Ok(new PlantOwnershipStatistics
        {
            UsedForPond = selectedForPond.Count(),
            OwnedByCount = allPlantOwnerships.Count,
            UsedByCount = selectedForPond.Count,
            OwnedByPercentageOfPopulation = (double)allPlantOwnerships.Count / totalUserCount,
            UsedForPondPercentageOfPopulation =  (double)selectedForPond.Count / totalUserCount,
            UsedByPercentageOfPopulation = (double)selectedForPond.Count / totalUserCount,
        });
        
    }
}