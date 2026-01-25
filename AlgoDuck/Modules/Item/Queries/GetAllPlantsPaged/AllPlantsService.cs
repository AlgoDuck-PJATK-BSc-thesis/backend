using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.Types;

namespace AlgoDuck.Modules.Item.Queries.GetAllPlantsPaged;

public interface IAllPlantsService
{
    public Task<Result<PageData<PlantItemDto>, ErrorObject<string>>> GetAllPlantsPagedAsync(PagedRequestWithAttribution pagedRequest, CancellationToken cancellationToken = default);
    
}

public class AllPlantsService : IAllPlantsService
{
    private readonly IAllPlantsRepository _allPlantsRepository;

    public AllPlantsService(IAllPlantsRepository allPlantsRepository)
    {
        _allPlantsRepository = allPlantsRepository;
    }

    public async Task<Result<PageData<PlantItemDto>, ErrorObject<string>>> GetAllPlantsPagedAsync(PagedRequestWithAttribution pagedRequest, CancellationToken cancellationToken = default)
    {
        return await _allPlantsRepository.GetAllPlantsPagedAsync(pagedRequest, cancellationToken);
    }
}