using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;

public interface IAllDucksService
{
    public Task<Result<PageData<DuckItemDto>, ErrorObject<string>>> GetAllDucksPagedAsync(PagedRequestWAttribution pagedRequest, CancellationToken cancellationToken = default);
    
}

public class AllDucksService(
    IAllDucksRepository allDucksRepository
    ) : IAllDucksService
{
    public async Task<Result<PageData<DuckItemDto>, ErrorObject<string>>> GetAllDucksPagedAsync(PagedRequestWAttribution pagedRequest, CancellationToken cancellationToken = default)
    {
        return await allDucksRepository.GetAllDucksPagedAsync(pagedRequest, cancellationToken);
    }
}