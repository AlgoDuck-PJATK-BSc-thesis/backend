using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Shared.Result;
using AlgoDuck.Shared.Types;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.AdminGetAllItemsPagedFilterable;

public interface IAllItemsPagedRepository
{
    public Task<Result<PageData<ItemDto>, NotFoundError<string>>> GetAllItemsPagedAsync(PagedRequestWithAttribution<ColumnFilterRequest<FetchableColumn>> itemRequest, CancellationToken cancellationToken = default);
}

public class AllItemsPagedRepository : IAllItemsPagedRepository
{
    private readonly ApplicationQueryDbContext _dbContext;

    public AllItemsPagedRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PageData<ItemDto>, NotFoundError<string>>> GetAllItemsPagedAsync(PagedRequestWithAttribution<ColumnFilterRequest<FetchableColumn>> itemRequest, CancellationToken cancellationToken = default)
    {
        var totalItemCount = await _dbContext.Items.CountAsync(cancellationToken);
        var totalPagesCount = (int) Math.Ceiling(totalItemCount / (double)itemRequest.PageSize);
        
        if (totalPagesCount <= 0)
            return Result<PageData<ItemDto>, NotFoundError<string>>.Err(new NotFoundError<string>("No items found"));
        
        var actualPage = Math.Clamp(itemRequest.CurrPage, 1, totalPagesCount);
        
        var itemQueryBase = _dbContext.Items
            .Include(i => i.Purchases);

        var itemsOrdered = itemRequest.FurtherData.OrderBy switch
        {
            FetchableColumn.ItemId => itemQueryBase.OrderByDescending(i => i.ItemId),
            FetchableColumn.ItemName => itemQueryBase.OrderByDescending(i => i.Name),
            FetchableColumn.CreatedOn => itemQueryBase.OrderByDescending(i => i.CreatedAt),
            FetchableColumn.CreatedBy => itemQueryBase.OrderByDescending(i => i.CreatedBy),
            FetchableColumn.OwnedCount => itemQueryBase.OrderByDescending(i => i.Purchases.Count),
            FetchableColumn.Type => itemQueryBase.OrderByDescending(i => EF.Property<string>(i, "type")),
            _ => itemQueryBase.OrderBy(i => i.ItemId)
        };

        var itemQueryPaged = itemsOrdered
            .Skip((actualPage - 1) * itemRequest.PageSize)
            .Take(itemRequest.PageSize);
        
        var itemQuerySelected = itemQueryPaged.Select(i => new ItemDto
        {
            CreatedOn = itemRequest.FurtherData.Fields.Contains(FetchableColumn.CreatedOn) ? DateTime.UtcNow : null,
            ItemId = i.ItemId,
            ItemName = itemRequest.FurtherData.Fields.Contains(FetchableColumn.ItemName) ? i.Name : null,
            CreatedBy = itemRequest.FurtherData.Fields.Contains(FetchableColumn.CreatedBy) ? i.CreatedById : null,
            OwnedCount = itemRequest.FurtherData.Fields.Contains(FetchableColumn.OwnedCount)
                ? i.Purchases.Count
                : null,
            Type = itemRequest.FurtherData.Fields.Contains(FetchableColumn.Type)
                ? EF.Property<string>(i, "type")
                : null,
        });
        
        return Result<PageData<ItemDto>, NotFoundError<string>>.Ok(new PageData<ItemDto>
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
