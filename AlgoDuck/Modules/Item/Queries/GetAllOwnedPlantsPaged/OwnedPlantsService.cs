using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.Types;

namespace AlgoDuck.Modules.Item.Queries.GetAllOwnedPlantsPaged;

public interface IOwnedPlantsService
{
    public Task<Result<PageData<OwnedPlantDto>, ErrorObject<string>>> GetOwnedPlantsAsync(
        OwnedItemsRequest ownedItemsRequest, CancellationToken cancellationToken = default);

}

public class OwnedPlantsService : IOwnedPlantsService
{
    private readonly IOwnedPlantsRepository _ownedPlantsRepository;

    public OwnedPlantsService(IOwnedPlantsRepository ownedPlantsRepository)
    {
        _ownedPlantsRepository = ownedPlantsRepository;
    }

    public async Task<Result<PageData<OwnedPlantDto>, ErrorObject<string>>> GetOwnedPlantsAsync(OwnedItemsRequest ownedItemsRequest, CancellationToken cancellationToken = default)
    {
        return await _ownedPlantsRepository.GetOwnedPlantsAsync(ownedItemsRequest, cancellationToken);
    }
}