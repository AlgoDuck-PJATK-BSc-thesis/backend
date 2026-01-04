using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetAllItemPages;

public interface IAllItemsPagedRepository
{
    public Task<Result<PageData<ItemDto>, ErrorObject<string>>> GetAllItemsPagedAsync(PagedRequestWithAttribution itemRequest, CancellationToken cancellationToken = default);
}

public class AllItemsPagedRepository(
    ApplicationQueryDbContext dbContext
) : IAllItemsPagedRepository
{
    public async Task<Result<PageData<ItemDto>, ErrorObject<string>>> GetAllItemsPagedAsync(PagedRequestWithAttribution itemRequest, CancellationToken cancellationToken = default)
    {
        var totalItemCount = await dbContext.Items.CountAsync(cancellationToken);
        var totalPagesCount = (int) Math.Ceiling(totalItemCount / (double)itemRequest.PageSize);
        
        if (totalPagesCount <= 0)
            return Result<PageData<ItemDto>, ErrorObject<string>>.Err(ErrorObject<string>.NotFound("No items found"));
        
        var actualPage = Math.Clamp(itemRequest.CurrPage, 1, totalPagesCount);

        Console.WriteLine(actualPage < totalItemCount ? actualPage + 1 : null);
        return Result<PageData<ItemDto>, ErrorObject<string>>.Ok(new PageData<ItemDto>
        {
            CurrPage = actualPage,
            TotalItems = totalItemCount,
            PageSize = itemRequest.PageSize,
            NextCursor = actualPage < totalPagesCount ? actualPage + 1 : null,
            PrevCursor = actualPage > 1 ? actualPage - 1 : null,
            Items = await dbContext.Items
                .OrderBy(i => i.ItemId) /* TODO: replace this with createdAt */     
                .Skip((actualPage - 1) * itemRequest.PageSize)
                .Take(itemRequest.PageSize)
                .Select(i => new ItemDto
                {
                    CreatedOn = DateTime.UtcNow,
                    Id = i.ItemId,
                    ItemName = i.Name,
                    CreatedBy = Guid.NewGuid()/*i.CreatedById*/
                })
                .ToListAsync(cancellationToken: cancellationToken)
        });
    }
}
