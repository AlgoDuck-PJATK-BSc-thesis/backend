using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.Types;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.AdminGetProblemDetailsPaged;

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

        var problemQueryBase = _dbContext.Problems
            .Include(i => i.Category)
            .Include(i => i.Difficulty)
            .Include(i => i.CodeExecutionStatistics)
            .Include(i => i.CreatedByUser);

        var problemsOrdered = columnFilterRequest.FurtherData.OrderBy switch
        {
            FetchableColumn.CreatedOn => problemQueryBase.OrderBy(i => i.CreatedAt),
            FetchableColumn.Category => problemQueryBase.OrderBy(i => i.Category.CategoryName),
            FetchableColumn.CreatedBy => problemQueryBase.OrderBy(i => i.CreatedByUserId),
            FetchableColumn.CompletionRatio => problemQueryBase.OrderBy(i =>
                i.CodeExecutionStatistics.Count == 0
                    ? 0f
                    : (float)i.CodeExecutionStatistics.Count(e => e.TestCaseResult == TestCaseResult.Accepted) / i.CodeExecutionStatistics.Count),
            FetchableColumn.Difficulty => problemQueryBase.OrderBy(i => i.Difficulty.DifficultyName),
            FetchableColumn.ProblemId => problemQueryBase.OrderBy(i => i.ProblemId),
            FetchableColumn.Name => problemQueryBase.OrderBy(i => i.ProblemTitle),
            _ => problemQueryBase.OrderBy(i => i.CreatedAt) 
        };

        var problemQueryPaged = problemsOrdered
            .Skip((actualPage - 1) * columnFilterRequest.PageSize)
            .Take(columnFilterRequest.PageSize);

        var problemQuerySelected = problemQueryPaged.Select(i => new ProblemDetailsDto
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
                      i.CodeExecutionStatistics.Count)
                : null,
            CreatedOn = columnFilterRequest.FurtherData.Fields.Contains(FetchableColumn.CreatedOn) ? i.CreatedAt : null,
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