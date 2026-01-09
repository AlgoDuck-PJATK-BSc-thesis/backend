using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;
using OneOf.Types;

namespace AlgoDuck.Modules.Item.Commands.RemovePlantFromHomepage;


public interface IRemovePlantRepository
{
    public Task<Result<Guid, ErrorObject<string>>> RemovePlantFromHomepageAsync(RemovePlantDto removePlantDto, CancellationToken cancellationToken = default);
}

public class RemovePlantRepository(
    ApplicationCommandDbContext dbContext
    ) : IRemovePlantRepository
{
    public async Task<Result<Guid, ErrorObject<string>>> RemovePlantFromHomepageAsync(RemovePlantDto removePlantDto, CancellationToken cancellationToken = default)
    {
        var rowsChanged = await dbContext.PlantOwnerships
            .Where(p => p.UserId == removePlantDto.UserId && p.ItemId == removePlantDto.ItemId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(io => io.GridX,(byte?) null).SetProperty(io => io.GridY,(byte?) null), cancellationToken: cancellationToken);
        
        if (rowsChanged == 0)
            return Result<Guid, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Could not attribute item {removePlantDto.ItemId} to user {removePlantDto.UserId}"));
        
        return Result<Guid, ErrorObject<string>>.Ok(removePlantDto.ItemId);
    }
}