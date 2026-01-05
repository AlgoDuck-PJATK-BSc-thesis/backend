using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetUserSolutionsForProblem;

public interface IUserSolutionRepository
{
    public Task<Result<PageData<UserSolutionDto>, ErrorObject<string>>> GetAllUserSolutionsAsync(UserSolutionRequestDto userSolutionRequestDto, CancellationToken cancellationToken = default);
    
}

public class UserSolutionRepository(
    ApplicationQueryDbContext dbContext
    ) : IUserSolutionRepository
{
    public async Task<Result<PageData<UserSolutionDto>, ErrorObject<string>>> GetAllUserSolutionsAsync(UserSolutionRequestDto userSolutionRequestDto,
        CancellationToken cancellationToken = default)
    {
        
        var totalItems = await dbContext.UserSolutions
            .Where(u => u.ProblemId == userSolutionRequestDto.ProblemId && u.UserId == userSolutionRequestDto.PagedRequestWithAttribution.UserId)
            .CountAsync(cancellationToken: cancellationToken);
        
        var totalPages = Math.Max(1, (int) Math.Ceiling((float)totalItems / userSolutionRequestDto.PagedRequestWithAttribution.PageSize));
        var actualPage = Math.Clamp(userSolutionRequestDto.PagedRequestWithAttribution.CurrPage, 1,  totalPages);
        
        return Result<PageData<UserSolutionDto>, ErrorObject<string>>.Ok(new PageData<UserSolutionDto>
        {
            CurrPage = actualPage,
            TotalItems = totalItems,
            PageSize = userSolutionRequestDto.PagedRequestWithAttribution.PageSize,
            NextCursor = actualPage < totalPages ? actualPage + 1 : null,
            PrevCursor = actualPage > 1 ? actualPage - 1 : null,
            Items = await dbContext.UserSolutions
                .Where(u => u.ProblemId == userSolutionRequestDto.ProblemId && u.UserId == userSolutionRequestDto.PagedRequestWithAttribution.UserId)
                .OrderBy(u => u.CreatedAt)
                .Skip((actualPage - 1) * userSolutionRequestDto.PagedRequestWithAttribution.PageSize)
                .Take(userSolutionRequestDto.PagedRequestWithAttribution.PageSize)
                .Select(u => new UserSolutionDto
                {
                    CodeRuntimeSubmitted = u.CodeRuntimeSubmitted,
                    CreatedAt = u.CreatedAt,
                    UserSolutionId = u.ProblemId,
                }).ToListAsync(cancellationToken: cancellationToken)
        });
    }
}