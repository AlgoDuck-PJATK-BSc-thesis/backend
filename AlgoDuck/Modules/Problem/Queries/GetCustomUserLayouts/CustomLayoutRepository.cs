using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetCustomUserLayouts;

public interface ICustomLayoutRepository
{
    public Task<Result<ICollection<LayoutDto>, ErrorObject<string>>> GetCustomLayoutsAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class CustomLayoutRepository(
    ApplicationQueryDbContext dbContext
    ) : ICustomLayoutRepository
{
    public async Task<Result<ICollection<LayoutDto>, ErrorObject<string>>> GetCustomLayoutsAsync(Guid userId, CancellationToken cancellationToken = default)
    {

        var result = await dbContext.UserConfigs.Include(u => u.EditorLayouts)
            .Where(u => u.UserId == userId)
            .SelectMany(u => u.EditorLayouts.Select(l => new LayoutDto
            {
                LayoutId = l.EditorLayoutId,
                LayoutName = l.LayoutName
            })).ToListAsync(cancellationToken: cancellationToken);
        
        
        return Result<ICollection<LayoutDto>, ErrorObject<string>>.Ok(result);
    }
}