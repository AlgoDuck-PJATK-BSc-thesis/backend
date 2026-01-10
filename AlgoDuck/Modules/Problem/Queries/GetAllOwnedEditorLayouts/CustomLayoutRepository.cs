using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetAllOwnedEditorLayouts;

public interface ICustomLayoutRepository
{
    public Task<Result<ICollection<LayoutDto>, ErrorObject<string>>> GetCustomLayoutsAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class CustomLayoutRepository : ICustomLayoutRepository
{

    private readonly ApplicationQueryDbContext _dbContext;

    public CustomLayoutRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ICollection<LayoutDto>, ErrorObject<string>>> GetCustomLayoutsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Result<ICollection<LayoutDto>, ErrorObject<string>>.Ok(
            await _dbContext.OwnsLayouts
                .Include(ol => ol.Layout)
                .Where(u => u.UserId == userId)
                .Select(ol => new LayoutDto
                {
                    LayoutId = ol.LayoutId,
                    LayoutName = ol.Layout.LayoutName
                }).ToListAsync(cancellationToken));
    }
}