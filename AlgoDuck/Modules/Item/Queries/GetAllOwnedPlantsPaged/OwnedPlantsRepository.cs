using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetAllOwnedPlantsPaged;

public interface IOwnedPlantsRepository
{
    public Task<Result<PageData<OwnedPlantDto>, ErrorObject<string>>> GetOwnedPlantsAsync(
        OwnedItemsRequest ownedItemsRequest, CancellationToken cancellationToken = default);
}

public class OwnedPlantsRepository(
    ApplicationQueryDbContext dbContext
    ) : IOwnedPlantsRepository
{
    public async Task<Result<PageData<OwnedPlantDto>, ErrorObject<string>>> GetOwnedPlantsAsync(OwnedItemsRequest ownedItemsRequest, CancellationToken cancellationToken = default)
    {
        var totalItems = await dbContext.DuckItems.CountAsync(cancellationToken: cancellationToken);

        var actualPage = Math.Clamp(ownedItemsRequest.CurrPage, 1, totalItems);

        var plantPage = await dbContext.PlantOwnerships.Where(o => o.UserId == ownedItemsRequest.UserId)
            .Include(e => e.Item)
            .Skip((actualPage - 1) * ownedItemsRequest.PageSize)
            .Take(ownedItemsRequest.PageSize)
            .ToListAsync(cancellationToken: cancellationToken);
        
        return Result<PageData<OwnedPlantDto>, ErrorObject<string>>.Ok(new PageData<OwnedPlantDto>
        {
            CurrPage = actualPage,
            TotalItems = totalItems,
            PrevCursor = actualPage > 1 ? actualPage - 1 : null,
            NextCursor = actualPage < totalItems ? actualPage + 1 : null,
            PageSize = ownedItemsRequest.PageSize,
            Items = plantPage 
                .Where(d => d.Item is PlantItem)
                .Select(d =>
                    new OwnedPlantDto
                    {
                        Height = (d.Item as PlantItem)!.Height,
                        Width = (d.Item as PlantItem)!.Width,
                        ItemId = d.ItemId,
                    }).ToList()
        });
    }
}