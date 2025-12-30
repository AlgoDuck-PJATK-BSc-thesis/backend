using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;

public interface IOwnedItemsRepository
{
    public Task<Result<OwnedItemsDto, ErrorObject<string>>> GetOwnedItemsByUserId(Guid userId,
        CancellationToken cancellationToken = default);
}

public class OwnedUsedItemsRepository(
    ApplicationQueryDbContext dbContext
) : IOwnedItemsRepository
{
    public async Task<Result<OwnedItemsDto, ErrorObject<string>>> GetOwnedItemsByUserId(Guid userId,
        CancellationToken cancellationToken = default)
    {
        
        var ownedPlantsRes =  await GetOwnedUsedPlants(userId, cancellationToken);
        if (ownedPlantsRes.IsErr)
            return Result<OwnedItemsDto, ErrorObject<string>>.Err(ownedPlantsRes.AsT1);
        
        var ownedDucksRes =  await GetOwnedUsedDucks(userId, cancellationToken);
        if (ownedDucksRes.IsErr)
            return Result<OwnedItemsDto, ErrorObject<string>>.Err(ownedDucksRes.AsT1);

        return Result<OwnedItemsDto, ErrorObject<string>>.Ok(new OwnedItemsDto
        {
            Ducks = ownedDucksRes.AsT0,
            Plants = ownedPlantsRes.AsT0
        });
    }

    private async Task<Result<ICollection<OwnedDuckItemDto>, ErrorObject<string>>> GetOwnedUsedDucks(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return Result<ICollection<OwnedDuckItemDto>, ErrorObject<string>>.Ok(await dbContext.DuckOwnerships
            .Where(e => e.UserId == userId && e.SelectedForPond).Select(d => new OwnedDuckItemDto()
            {
                ItemId = d.ItemId,
                IsSelectedAsAvatar = d.SelectedAsAvatar,
                IsSelectedForPond = d.SelectedForPond,
            }).ToListAsync(cancellationToken));
    }

    private async Task<Result<ICollection<OwnedPlantItemDto>, ErrorObject<string>>> GetOwnedUsedPlants(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var ownedPlants = await dbContext.PlantOwnerships
            .Include(e => e.Item)
            .Where(e => e.UserId == userId && e.GridX != null && e.GridY != null)
            .ToListAsync(cancellationToken);
        
        
        return Result<ICollection<OwnedPlantItemDto>, ErrorObject<string>>.Ok(ownedPlants.Where(e => e.Item is PlantItem).Select(e => new OwnedPlantItemDto()
        {
            ItemId = e.ItemId,
            GridX = (byte) e.GridX!,
            GridY = (byte) e.GridY!,
            Height = (e.Item as PlantItem)!.Height,
            Width = (e.Item as PlantItem)!.Width
        }).ToList());
    }
}