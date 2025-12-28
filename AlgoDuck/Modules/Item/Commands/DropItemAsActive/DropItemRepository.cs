using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Commands.DropItemAsActive;

public interface IDropItemRepository
{
    public Task<Result<DeselectItemResultDto, ErrorObject<string>>> DeselectItemAsync(DeselectItemDto dto,
        CancellationToken token = default);
}

public class DropItemRepository(
    ApplicationCommandDbContext dbContext
) : IDropItemRepository
{
    public async Task<Result<DeselectItemResultDto, ErrorObject<string>>> DeselectItemAsync(DeselectItemDto dto,
        CancellationToken token = default)
    {
        var rowsChanged = await dbContext.Purchases
            .Where(p => p.ItemId == dto.ItemId && p.UserId == dto.UserId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Selected, false), cancellationToken: token);
        
        if (rowsChanged == 0)
            return Result<DeselectItemResultDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Item with id {dto.ItemId} not found"));
        
        return Result<DeselectItemResultDto, ErrorObject<string>>.Ok(new DeselectItemResultDto
        {
            ItemId = dto.ItemId
        });
    }
}