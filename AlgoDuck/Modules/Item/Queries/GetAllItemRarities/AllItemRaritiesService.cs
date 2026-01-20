using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetAllItemRarities;

public interface IAllItemRaritiesService
{
    public Task<Result<ICollection<ItemRarityDto>, ErrorObject<string>>> GetAllRaritiesAsync(CancellationToken cancellationToken = default);
}

public class AllItemRaritiesService(
    IAllItemRaritiesRepository allItemRaritiesRepository
) : IAllItemRaritiesService
{
    public async Task<Result<ICollection<ItemRarityDto>, ErrorObject<string>>> GetAllRaritiesAsync(CancellationToken cancellationToken = default)
    {
        return await allItemRaritiesRepository.GetAllRaritiesAsync(cancellationToken);
    }
}
