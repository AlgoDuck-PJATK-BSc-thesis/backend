using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetAllPlantsPaged;


public interface IAllPlantsRepository
{
    public Task<Result<PageData<PlantItemDto>, ErrorObject<string>>> GetAllPlantsPagedAsync(PagedRequestWAttribution pagedRequest, CancellationToken cancellationToken = default);
}

public class AllPlantsRepository(
    ApplicationQueryDbContext dbContext
    ) : IAllPlantsRepository
{
    public async Task<Result<PageData<PlantItemDto>, ErrorObject<string>>> GetAllPlantsPagedAsync(PagedRequestWAttribution pagedRequest, CancellationToken cancellationToken = default)
    {
        var totalItems = await dbContext.PlantItems.CountAsync(cancellationToken);

        var pageCount = (int) Math.Ceiling((float) totalItems / pagedRequest.PageSize);
        
        var actualPage = Math.Clamp(pagedRequest.CurrPage, 1,  pageCount);
        
        return Result<PageData<PlantItemDto>, ErrorObject<string>>.Ok(new PageData<PlantItemDto>
        {
            CurrPage = actualPage,
            PageSize = pagedRequest.PageSize,
            TotalItems = totalItems,
            NextCursor = actualPage < pageCount ? actualPage + 1 : null,
            PrevCursor = actualPage > 1 ? actualPage - 1 : null,
            Items = await dbContext.PlantItems
                .Include(i => i.Purchases).ThenInclude(p => p.User)
                .Include(i => i.Rarity)
                .Where(i => i.Purchasable)
                .Skip(pagedRequest.PageSize * (actualPage - 1))
                .Take(pagedRequest.PageSize)
                .Select(i => new PlantItemDto
                {
                    ItemId = i.ItemId,
                    Name = i.Name,
                    Price = i.Price,
                    IsOwned = i.Purchases.Any(p => p.UserId == pagedRequest.UserId),
                    ItemRarity = new ItemRarityDto
                    {
                        RarityName = i.Rarity.RarityName,
                    },
                    Width = i.Width,
                    Height = i.Height,
                }).ToListAsync(cancellationToken: cancellationToken),
        });
    }
}

public class ItemDto
{
    public required Guid ItemId { get; set; }
    public required string Name { get; set; }
    public required int Price { get; set; }
    public required bool IsOwned { get; set; }
    public string? Description { get; set; }
    public required ItemRarityDto ItemRarity { get; set; }
}
public class ItemRarityDto
{
    public required string RarityName { get; set; }
}