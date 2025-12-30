using AlgoDuck.Modules.Item.Queries.GetOwnedDucksPaged;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetAllOwnedPlantsPaged;

public interface IOwnedPlantsService
{
    public Task<Result<PageData<OwnedPlantDto>, ErrorObject<string>>> GetOwnedPlantsAsync(
        OwnedItemsRequest ownedItemsRequest, CancellationToken cancellationToken = default);

}

public class OwnedPlantsService(
    IOwnedPlantsRepository ownedPlantsRepository
    ) : IOwnedPlantsService
{
    public async Task<Result<PageData<OwnedPlantDto>, ErrorObject<string>>> GetOwnedPlantsAsync(OwnedItemsRequest ownedItemsRequest, CancellationToken cancellationToken = default)
    {
        return await ownedPlantsRepository.GetOwnedPlantsAsync(ownedItemsRequest, cancellationToken);
    }
}