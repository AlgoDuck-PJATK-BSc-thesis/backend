using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemDetailsPagedAdmin;

public interface IPagedProblemDetailsAdminService
{
    public Task<Result<PageData<ProblemDetailsDto>, ErrorObject<string>>> GetProblemDetailsAsync(PagedRequestWithAttribution<ColumnFilterRequest<FetchableColumn>> columnFilterRequest, CancellationToken cancellationToken = default);
}

public class PagedProblemDetailsAdminService : IPagedProblemDetailsAdminService
{
    private readonly IPagedProblemDetailsAdminRepository _pagedProblemDetailsAdminRepository;

    public PagedProblemDetailsAdminService(IPagedProblemDetailsAdminRepository pagedProblemDetailsAdminRepository)
    {
        _pagedProblemDetailsAdminRepository = pagedProblemDetailsAdminRepository;
    }

    public async Task<Result<PageData<ProblemDetailsDto>, ErrorObject<string>>> GetProblemDetailsAsync(PagedRequestWithAttribution<ColumnFilterRequest<FetchableColumn>> columnFilterRequest,
        CancellationToken cancellationToken = default)
    {
        return await _pagedProblemDetailsAdminRepository.GetProblemDetailsAsync(columnFilterRequest, cancellationToken);
    }
}