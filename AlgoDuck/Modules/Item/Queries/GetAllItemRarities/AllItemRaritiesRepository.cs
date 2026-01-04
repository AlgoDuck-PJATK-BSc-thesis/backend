using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetAllItemRarities;


public interface IAllItemRaritiesRepository
{
    public Task<Result<ICollection<ItemRarityDto>, ErrorObject<string>>> GetAllRaritiesAsync(CancellationToken cancellationToken = default);
}

public class AllItemRaritiesRepository(
    ApplicationQueryDbContext dbContext
) : IAllItemRaritiesRepository
{
    public async Task<Result<ICollection<ItemRarityDto>, ErrorObject<string>>> GetAllRaritiesAsync(CancellationToken cancellationToken = default)
    {
        return Result<ICollection<ItemRarityDto>, ErrorObject<string>>.Ok(await dbContext.Rarities.Select(d => new ItemRarityDto
        {
            RarityId = d.RarityId,
            Name = d.RarityName
        }).ToListAsync(cancellationToken: cancellationToken));
    }
}