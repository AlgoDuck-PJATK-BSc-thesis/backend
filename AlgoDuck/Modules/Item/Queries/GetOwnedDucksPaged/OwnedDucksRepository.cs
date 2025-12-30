using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetOwnedDucksPaged;

public interface IOwnedDucksRepository
{
    public Task<Result<PageData<OwnedDuckDto>, ErrorObject<string>>> GetOwnedItemsByTypePagedAsync(
        OwnedItemsRequest ownedItemsRequest, CancellationToken cancellationToken = default);
}

public class OwnedDucksRepository(
    ApplicationQueryDbContext dbContext
) : IOwnedDucksRepository
{
    public async Task<Result<PageData<OwnedDuckDto>, ErrorObject<string>>> GetOwnedItemsByTypePagedAsync(
        OwnedItemsRequest ownedItemsRequest, CancellationToken cancellationToken = default)
    {
        var totalItems = await dbContext.DuckItems.CountAsync(cancellationToken: cancellationToken);

        var actualPage = Math.Clamp(ownedItemsRequest.CurrPage, 1, totalItems);

        return Result<PageData<OwnedDuckDto>, ErrorObject<string>>.Ok(new PageData<OwnedDuckDto>()
        {
            CurrPage = actualPage,
            TotalItems = totalItems,
            PrevCursor = actualPage > 1 ? actualPage - 1 : null,
            NextCursor = actualPage < totalItems ? actualPage + 1 : null,
            PageSize = ownedItemsRequest.PageSize,
            Items = await dbContext.DuckOwnerships.Where(o => o.UserId == ownedItemsRequest.UserId)
                .Skip((actualPage - 1) * ownedItemsRequest.PageSize).Take(ownedItemsRequest.PageSize).Select(d =>
                    new OwnedDuckDto
                    {
                        IsSelectedAsAvatar = d.SelectedAsAvatar,
                        IsSelectedForPond = d.SelectedForPond,
                        ItemId = d.ItemId,
                    }).ToListAsync(cancellationToken)
        });
    }
}