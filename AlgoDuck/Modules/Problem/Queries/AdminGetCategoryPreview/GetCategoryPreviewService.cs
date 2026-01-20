using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.AdminGetCategoryPreview;

public interface IGetCategoryPreviewService
{
    public Task<Result<CategoryPreviewDto, ErrorObject<string>>> GetCategoryPreviewAsync(Guid categoryId, CancellationToken cancellationToken = default);
}

public class GetCategoryPreviewService : IGetCategoryPreviewService
{
    private readonly IGetCategoryPreviewRepository _categoryPreviewRepository;

    public GetCategoryPreviewService(IGetCategoryPreviewRepository categoryPreviewRepository)
    {
        _categoryPreviewRepository = categoryPreviewRepository;
    }

    public async Task<Result<CategoryPreviewDto, ErrorObject<string>>> GetCategoryPreviewAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _categoryPreviewRepository.GetCategoryPreviewAsync(categoryId, cancellationToken);
    }
}
