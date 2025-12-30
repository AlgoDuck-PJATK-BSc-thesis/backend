using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;
using OneOf.Types;

namespace AlgoDuck.Modules.Item.Commands.SelectItemAsActive;

public interface ISelectItemRepository
{
    public Task<Result<SelectItemResultDto, ErrorObject<string>>> SelectItemAsync(SelectItemDto item, CancellationToken token = default);

    public Task<Result<int, ErrorObject<string>>> GetCurrentSelectedCountAsync(SelectItemDto item, CancellationToken token = default);
}

public class SelectItemRepository(
    ApplicationCommandDbContext dbContext
    ) : ISelectItemRepository
{
    
    public async Task<Result<SelectItemResultDto, ErrorObject<string>>> SelectItemAsync(SelectItemDto item, CancellationToken token = default)
    {
        var rowsChanged = await dbContext.DuckOwnerships
            .Where(p => p.UserId == item.UserId && p.ItemId == item.ItemId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.SelectedForPond, true), cancellationToken: token);
        
        if (rowsChanged == 0)
            return Result<SelectItemResultDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Item with id {item.ItemId} not found"));
        
        return Result<SelectItemResultDto, ErrorObject<string>>.Ok(new SelectItemResultDto
        {
            ItemId = item.ItemId
        });
    }

    public async Task<Result<int, ErrorObject<string>>> GetCurrentSelectedCountAsync(SelectItemDto item, CancellationToken token = default)
    {
        var count = await dbContext.DuckOwnerships.CountAsync(p => p.UserId == item.UserId && p.ItemId == item.ItemId && p.SelectedForPond,
            cancellationToken: token);
        return Result<int, ErrorObject<string>>.Ok(count);
    }
}