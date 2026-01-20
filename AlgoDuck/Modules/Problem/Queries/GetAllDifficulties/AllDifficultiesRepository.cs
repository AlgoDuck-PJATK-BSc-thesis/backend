using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetAllDifficulties;


public interface IAllDifficultiesRepository
{
    public Task<Result<ICollection<DifficultyDto>, ErrorObject<string>>> GetAllDifficultiesAsync(CancellationToken cancellationToken = default);
}

public class AllDifficultiesRepository : IAllDifficultiesRepository
{
    private readonly ApplicationQueryDbContext _dbContext;

    public AllDifficultiesRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ICollection<DifficultyDto>, ErrorObject<string>>> GetAllDifficultiesAsync(CancellationToken cancellationToken = default)
    {
        return Result<ICollection<DifficultyDto>, ErrorObject<string>>.Ok(
            await _dbContext.Difficulties
                .OrderBy(d => d.RewardScaler)
                .Select(d => new DifficultyDto
                {
                    Name = d.DifficultyName,
                    Id = d.DifficultyId
                }).ToListAsync(cancellationToken: cancellationToken));
    }
}