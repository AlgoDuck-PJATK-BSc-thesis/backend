using AlgoDuck.DAL;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.LoadProblemCreateFormStateFromProblem;

public interface IFormStateLoadRepository
{
    public Task<Result<FullProblemDataDto, ErrorObject<string>>> GetFullProblemDataAsync(Guid problemId, CancellationToken cancellationToken = default);
}

public class FormStateLoadLoadRepository : IFormStateLoadRepository
{
    private readonly ApplicationQueryDbContext _dbContext;

    public FormStateLoadLoadRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<FullProblemDataDto, ErrorObject<string>>> GetFullProblemDataAsync(Guid problemId, CancellationToken cancellationToken = default)
    {
        var problem = await _dbContext.Problems.Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.ProblemId == problemId, cancellationToken);    
        if (problem == null) 
            return Result<FullProblemDataDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"problem {problemId} not found"));
        
        return Result<FullProblemDataDto, ErrorObject<string>>.Ok(new FullProblemDataDto
        {
            ProblemName = problem.ProblemTitle,
            CategoryId = problem.CategoryId,
            DifficultyId = problem.DifficultyId,
            Tags = problem.Tags.Select(t => new TagDto()
            {
                TagName = t.TagName,
            }).ToList()
        });
    }
}