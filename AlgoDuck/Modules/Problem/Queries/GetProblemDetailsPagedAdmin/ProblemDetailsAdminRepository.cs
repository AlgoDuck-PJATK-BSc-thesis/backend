using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemDetailsPagedAdmin;

public interface IPagedProblemDetailsAdminRepository
{
    public Task<Result<PageData<ProblemDetailsDto>, ErrorObject<string>>> GetProblemDetailsAsync(
        PagedRequestWithAttribution<ColumnFilterRequest<FetchableColumn>> columnFilterRequest,
        CancellationToken cancellationToken = default);
}

public class PagedProblemDetailsAdminRepository : IPagedProblemDetailsAdminRepository
{
    private readonly ApplicationQueryDbContext _dbContext;

    public PagedProblemDetailsAdminRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PageData<ProblemDetailsDto>, ErrorObject<string>>> GetProblemDetailsAsync(
        PagedRequestWithAttribution<ColumnFilterRequest<FetchableColumn>> columnFilterRequest,
        CancellationToken cancellationToken = default)
    {
        var totalItemCount = await _dbContext.Problems.CountAsync(cancellationToken);
        var totalPagesCount = (int)Math.Ceiling(totalItemCount / (double)columnFilterRequest.PageSize);

        if (totalPagesCount <= 0)
            return Result<PageData<ProblemDetailsDto>, ErrorObject<string>>.Err(
                ErrorObject<string>.NotFound("No items found"));

        var actualPage = Math.Clamp(columnFilterRequest.CurrPage, 1, totalPagesCount);

        var problemQueryPaged = _dbContext.Problems
            .Include(i => i.Category)
            .Include(i => i.Difficulty)
            .Include(i => i.CodeExecutionStatistics)
            .Include(i => i.CreatedByUser)
            .OrderBy(i => i.CreatedAt)
            .Skip((actualPage - 1) * columnFilterRequest.PageSize)
            .Take(columnFilterRequest.PageSize);

        var problemsOrdered = columnFilterRequest.FurtherData.OrderBy switch
        {
            FetchableColumn.CreatedAt => problemQueryPaged.OrderBy(i => i.CreatedAt),
            FetchableColumn.Category => problemQueryPaged.OrderBy(i => i.Category.CategoryName),
            FetchableColumn.CreatedBy => problemQueryPaged.OrderBy(i => i.CreatedByUserId),
            FetchableColumn.CompletionRatio => problemQueryPaged.OrderBy(i =>
                i.CodeExecutionStatistics.Count == 0
                    ? 0f
                    : (float)i.CodeExecutionStatistics.Count(e => e.TestCaseResult == TestCaseResult.Accepted) /
                      i.CodeExecutionStatistics.Count),
            FetchableColumn.Difficulty => problemQueryPaged.OrderBy(i => i.Difficulty.DifficultyName),
            FetchableColumn.ProblemId => problemQueryPaged.OrderBy(i => i.ProblemId),
            FetchableColumn.Name => problemQueryPaged.OrderBy(i => i.ProblemTitle),
            _ => problemQueryPaged
        };

        var problemQuerySelected = problemsOrdered.Select(i => new ProblemDetailsDto
        {
            Category = columnFilterRequest.FurtherData.Fields.Contains(FetchableColumn.Category)
                ? new CategoryDto
                {
                    Name = i.Category.CategoryName,
                    CategoryId = i.CategoryId
                }
                : null,
            CompletionRatio = columnFilterRequest.FurtherData.Fields.Contains(FetchableColumn.CompletionRatio)
                ? (!i.CodeExecutionStatistics.Any()
                    ? 0f
                    : (float)i.CodeExecutionStatistics.Count(e => e.TestCaseResult == TestCaseResult.Accepted) /
                      i.CodeExecutionStatistics.Count())
                : null,
            CreatedAt = columnFilterRequest.FurtherData.Fields.Contains(FetchableColumn.CreatedAt) ? i.CreatedAt : null,
            CreatedBy = columnFilterRequest.FurtherData.Fields.Contains(FetchableColumn.CreatedBy) ? new CreatingUserDto
            {
                Id = i.CreatedByUserId,
                Username = i.CreatedByUser.UserName ?? "<undefined>"
            } : null,
            Difficulty = columnFilterRequest.FurtherData.Fields.Contains(FetchableColumn.Difficulty) ? new DifficultyDto()
            {
                DifficultyId = i.DifficultyId,
                Name = i.Difficulty.DifficultyName
            } : null,
            Name = columnFilterRequest.FurtherData.Fields.Contains(FetchableColumn.Name) ? i.ProblemTitle : null,
            ProblemId = i.ProblemId
        });

        return Result<PageData<ProblemDetailsDto>, ErrorObject<string>>.Ok(new PageData<ProblemDetailsDto>
        {
            CurrPage = actualPage,
            TotalItems = totalItemCount,
            PageSize = columnFilterRequest.PageSize,
            NextCursor = actualPage < totalPagesCount ? actualPage + 1 : null,
            PrevCursor = actualPage > 1 ? actualPage - 1 : null,
            Items = await problemQuerySelected.ToListAsync(cancellationToken: cancellationToken)
        });
    }
}