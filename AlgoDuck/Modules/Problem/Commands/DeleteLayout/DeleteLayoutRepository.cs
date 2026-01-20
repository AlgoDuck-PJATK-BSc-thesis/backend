using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.DeleteLayout;

public interface IDeleteLayoutRepository
{
    public Task<Result<DeleteLayoutResult, ErrorObject<string>>> DeleteLayoutAsync(DeleteLayoutRequest request, CancellationToken cancellationToken = default);
}

public class DeleteLayoutRepository : IDeleteLayoutRepository
{
    private readonly ApplicationCommandDbContext _dbContext;

    public DeleteLayoutRepository(ApplicationCommandDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DeleteLayoutResult, ErrorObject<string>>> DeleteLayoutAsync(DeleteLayoutRequest request, CancellationToken cancellationToken = default)
    {
        var rowsChanged = await _dbContext.EditorLayouts.Include(l => l.OwnedBy)
            .Where(l => l.OwnedBy.Select(ol => ol.UserId).Contains(request.RequestingUserId) &&
                        l.EditorLayoutId == request.LayoutId && !l.IsDefaultLayout)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);
        if (rowsChanged == 0)
            return Result<DeleteLayoutResult, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Could not attribute layout: {request.LayoutId} to user: {request.RequestingUserId}"));
        return Result<DeleteLayoutResult, ErrorObject<string>>.Ok(new DeleteLayoutResult
        {
            LayoutId = request.LayoutId,
        });
    }
}