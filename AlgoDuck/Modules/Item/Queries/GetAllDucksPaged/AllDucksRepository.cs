using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;


public interface IAllDucksRepository
{
    public Task<Result<PageData<DuckItemDto>, ErrorObject<string>>> GetAllDucksPagedAsync(PagedRequestWithAttribution pagedRequest, CancellationToken cancellationToken = default);
}

public class AllDucksRepository : IAllDucksRepository
{
    private readonly ApplicationQueryDbContext _dbContext;

    public AllDucksRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PageData<DuckItemDto>, ErrorObject<string>>> GetAllDucksPagedAsync(PagedRequestWithAttribution pagedRequest, CancellationToken cancellationToken = default)
    {
        var purchasableDucksQueryable = _dbContext.DuckItems.Where(d => d.Purchasable);
        var totalItems = await purchasableDucksQueryable.CountAsync(cancellationToken);

        var pageCount = (int) Math.Ceiling((float) totalItems / pagedRequest.PageSize);
        var actualPage = Math.Clamp(pagedRequest.CurrPage, 1,  pageCount);

        return Result<PageData<DuckItemDto>, ErrorObject<string>>.Ok(new PageData<DuckItemDto>
        {
            CurrPage = actualPage,
            PageSize = pagedRequest.PageSize,
            TotalItems = totalItems,
            NextCursor = actualPage < pageCount ? actualPage + 1 : null,
            PrevCursor = actualPage > 1 ? actualPage - 1 : null,
            Items = await purchasableDucksQueryable
                .Include(i => i.Purchases).ThenInclude(p => p.User)
                .Include(i => i.Rarity)
                .Where(i => i.Purchasable)
                .Skip(pagedRequest.PageSize * (actualPage - 1))
                .Take(pagedRequest.PageSize)
                .Select(i => new DuckItemDto()
                {
                    ItemId = i.ItemId,
                    Name = i.Name,
                    Price = i.Price,
                    IsOwned = i.Purchases.Any(p => p.UserId == pagedRequest.UserId),
                    ItemRarity = new ItemRarityDto
                    {
                        RarityName = i.Rarity.RarityName,
                    }
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