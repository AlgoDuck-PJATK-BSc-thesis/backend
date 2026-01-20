using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.DeleteLayout;
[Route("api/problem/layout")]
[ApiController]
[Authorize]
public partial class LayoutController : ControllerBase
{
    private readonly IDeleteLayoutService _deleteLayoutService;

    public LayoutController(IDeleteLayoutService deleteLayoutService)
    {
        _deleteLayoutService = deleteLayoutService;
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteLayoutAsync([FromQuery] Guid layoutId, CancellationToken cancellationToken)
    {
        return await User.GetUserId().BindAsync(async userId => await _deleteLayoutService.DeleteLayoutAsync(new DeleteLayoutRequest()
        {
            LayoutId = layoutId,
            RequestingUserId = userId
        }, cancellationToken)).ToActionResultAsync();
    }
}

public class DeleteLayoutRequest
{
    public required Guid LayoutId { get; init; }
    internal Guid RequestingUserId { get; set; }
}

public class DeleteLayoutResult
{
    public required Guid LayoutId { get; init; }
}

public interface IDeleteLayoutService
{
    public Task<Result<DeleteLayoutResult, ErrorObject<string>>> DeleteLayoutAsync(DeleteLayoutRequest request, CancellationToken cancellationToken = default);
}

public class DeleteLayoutService : IDeleteLayoutService
{
    private readonly IDeleteLayoutRepository _deleteLayoutRepository;

    public DeleteLayoutService(IDeleteLayoutRepository deleteLayoutRepository)
    {
        _deleteLayoutRepository = deleteLayoutRepository;
    }

    public async Task<Result<DeleteLayoutResult, ErrorObject<string>>> DeleteLayoutAsync(DeleteLayoutRequest request, CancellationToken cancellationToken = default)
    {
        return await _deleteLayoutRepository.DeleteLayoutAsync(request, cancellationToken);
    }
}

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