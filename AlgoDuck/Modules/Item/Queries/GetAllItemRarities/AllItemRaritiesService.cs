using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetAllItemRarities;

public interface IAllItemRaritiesService
{
    public Task<Result<ICollection<ItemRarityDto>, ErrorObject<string>>> GetAllRaritiesAsync(CancellationToken cancellationToken = default);
}

public class AllItemRaritiesService : IAllItemRaritiesService
{
    private readonly IAllItemRaritiesRepository _allItemRaritiesRepository;

    public AllItemRaritiesService(IAllItemRaritiesRepository allItemRaritiesRepository)
    {
        _allItemRaritiesRepository = allItemRaritiesRepository;
    }

    public async Task<Result<ICollection<ItemRarityDto>, ErrorObject<string>>> GetAllRaritiesAsync(CancellationToken cancellationToken = default)
    {
        return await _allItemRaritiesRepository.GetAllRaritiesAsync(cancellationToken);
    }
}
