using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetAllPlantsPaged;

public interface IAllPlantsService
{
    public Task<Result<PageData<PlantItemDto>, ErrorObject<string>>> GetAllPlantsPagedAsync(PagedRequestWAttribution pagedRequest, CancellationToken cancellationToken = default);
    
}

public class AllPlantsService(
    IAllPlantsRepository allPlantsRepository
    ) : IAllPlantsService
{
    public async Task<Result<PageData<PlantItemDto>, ErrorObject<string>>> GetAllPlantsPagedAsync(PagedRequestWAttribution pagedRequest, CancellationToken cancellationToken = default)
    {
        return await allPlantsRepository.GetAllPlantsPagedAsync(pagedRequest, cancellationToken);
    }
}