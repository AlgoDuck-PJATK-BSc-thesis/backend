using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;

public interface IAllDucksService
{
    public Task<Result<PageData<DuckItemDto>, ErrorObject<string>>> GetAllDucksPagedAsync(PagedRequestWithAttribution pagedRequest, CancellationToken cancellationToken = default);
    
}

public class AllDucksService : IAllDucksService
{
    private readonly IAllDucksRepository _allDucksRepository;

    public AllDucksService(IAllDucksRepository allDucksRepository)
    {
        _allDucksRepository = allDucksRepository;
    }

    public async Task<Result<PageData<DuckItemDto>, ErrorObject<string>>> GetAllDucksPagedAsync(PagedRequestWithAttribution pagedRequest, CancellationToken cancellationToken = default)
    {
        return await _allDucksRepository.GetAllDucksPagedAsync(pagedRequest, cancellationToken);
    }
}