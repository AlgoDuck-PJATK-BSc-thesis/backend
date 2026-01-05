using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetAllItemPaged;

public interface IAllItemsPagedRepository
{
    public Task<Result<PageData<ItemDto>, ErrorObject<string>>> GetAllItemsPagedAsync(PagedRequestWithAttribution<ColumnFilterRequest> itemRequest, CancellationToken cancellationToken = default);
}

public class AllItemsPagedRepository(
    ApplicationQueryDbContext dbContext
) : IAllItemsPagedRepository
{
    public async Task<Result<PageData<ItemDto>, ErrorObject<string>>> GetAllItemsPagedAsync(PagedRequestWithAttribution<ColumnFilterRequest> itemRequest, CancellationToken cancellationToken = default)
    {
        var totalItemCount = await dbContext.Items.CountAsync(cancellationToken);
        var totalPagesCount = (int) Math.Ceiling(totalItemCount / (double)itemRequest.PageSize);
        
        if (totalPagesCount <= 0)
            return Result<PageData<ItemDto>, ErrorObject<string>>.Err(ErrorObject<string>.NotFound("No items found"));
        
        var actualPage = Math.Clamp(itemRequest.CurrPage, 1, totalPagesCount);

        var itemQueryPaged = dbContext.Items
            .Include(i => i.Purchases)
            .OrderBy(i => i.ItemId) /* TODO: replace this with createdAt */
            .Skip((actualPage - 1) * itemRequest.PageSize)
            .Take(itemRequest.PageSize);


        var itemsOrdered = itemRequest.FurtherData.OrderBy switch
        {
            FetchableColumn.ItemId => itemQueryPaged.OrderBy(i => i.ItemId),
            FetchableColumn.ItemName => itemQueryPaged.OrderBy(i => i.Name),
            FetchableColumn.CreatedOn => itemQueryPaged.OrderBy(i => i.CreatedAt),
            FetchableColumn.CreatedBy => itemQueryPaged.OrderBy(i => i.CreatedBy),
            FetchableColumn.OwnedCount => itemQueryPaged.OrderBy(i => i.Purchases.Count),
            // ReSharper disable once EntityFramework.ClientSideDbFunctionCall
            FetchableColumn.Type => itemQueryPaged.OrderBy(i => EF.Property<string>(i, "type")),
            _ => itemQueryPaged
        };
        
        var itemQuerySelected = itemsOrdered.Select(i => new ItemDto
        {
            CreatedOn = itemRequest.FurtherData.Fields.Contains(FetchableColumn.CreatedBy) ? i.CreatedAt : null,
            ItemId = i.ItemId,
            ItemName = itemRequest.FurtherData.Fields.Contains(FetchableColumn.ItemId) ? i.Name : null,
            CreatedBy = itemRequest.FurtherData.Fields.Contains(FetchableColumn.CreatedBy) ? i.CreatedById : null,
            OwnedCount = itemRequest.FurtherData.Fields.Contains(FetchableColumn.CreatedBy)
                ? i.Purchases.Count
                : null,
            Type = itemRequest.FurtherData.Fields.Contains(FetchableColumn.Type)
                ? EF.Property<string>(i, "type")
                : null,
        });
        
        return Result<PageData<ItemDto>, ErrorObject<string>>.Ok(new PageData<ItemDto>
        {
            CurrPage = actualPage,
            TotalItems = totalItemCount,
            PageSize = itemRequest.PageSize,
            NextCursor = actualPage < totalPagesCount ? actualPage + 1 : null,
            PrevCursor = actualPage > 1 ? actualPage - 1 : null,
            Items = await itemQuerySelected.ToListAsync(cancellationToken: cancellationToken)
        });
    }
}
