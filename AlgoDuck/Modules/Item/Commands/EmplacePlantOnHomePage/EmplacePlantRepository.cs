using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;
using OneOf.Types;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace AlgoDuck.Modules.Item.Commands.EmplacePlantOnHomePage;

public interface IEmplacePlantRepository
{
    public Task<Result<EmplacePlantResult, ErrorObject<string>>> EmplacePlantAsync(EmplacePlantDto emplacePlantDto,
        CancellationToken cancellationToken = default);

    public Task<Result<bool, ErrorObject<string>>> DoesUserOwnItemAsync(EmplacePlantDto emplacePlantDto,
        CancellationToken cancellationToken = default);
}

public class EmplacePlantRepository(
    ApplicationCommandDbContext dbContext
) : IEmplacePlantRepository
{
    public async Task<Result<EmplacePlantResult, ErrorObject<string>>> EmplacePlantAsync(
        EmplacePlantDto emplacePlantDto, CancellationToken cancellationToken = default)
    {
        var rowsChanged = await dbContext.PlantOwnerships
            .Where(e => e.UserId == emplacePlantDto.UserId && e.ItemId == emplacePlantDto.ItemId)
            .ExecuteUpdateAsync(
                setters =>
                    setters.SetProperty(e => e.GridX, emplacePlantDto.GridX)
                        .SetProperty(e => e.GridY, emplacePlantDto.GridY), cancellationToken: cancellationToken);
        
        if (rowsChanged == 0)
            return Result<EmplacePlantResult, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest($"This should not be happening. itemId: {emplacePlantDto.ItemId}; userId: {emplacePlantDto.UserId}"));
        return Result<EmplacePlantResult, ErrorObject<string>>.Ok(new EmplacePlantResult
        {
            ItemId =  emplacePlantDto.ItemId
        });
    }

    public async Task<Result<bool, ErrorObject<string>>> DoesUserOwnItemAsync(EmplacePlantDto emplacePlantDto,
        CancellationToken cancellationToken = default)
    {
        var ownedItemsCount = await dbContext.PlantOwnerships.CountAsync(
            e => e.UserId == emplacePlantDto.UserId && e.ItemId == emplacePlantDto.ItemId,
            cancellationToken: cancellationToken);
        if (ownedItemsCount == 0)
        {
            return Result<bool, ErrorObject<string>>.Err(
                ErrorObject<string>.BadRequest($"Cannot emplace {emplacePlantDto.ItemId}. Item not ownd"));
        }

        return Result<bool, ErrorObject<string>>.Ok(true);
    }
}