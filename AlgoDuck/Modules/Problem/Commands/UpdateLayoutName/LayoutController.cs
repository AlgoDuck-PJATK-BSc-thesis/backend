using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.UpdateLayoutName;

[Route("api/problem/layout")]
[ApiController]
[Authorize]
public partial class LayoutController : ControllerBase
{
    private readonly IUpdateLayoutNameService _updateLayoutNameService;

    public LayoutController(IUpdateLayoutNameService updateLayoutNameService)
    {
        _updateLayoutNameService = updateLayoutNameService;
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateChatNameAsync([FromBody] RenameLayoutRequestDto request,
        CancellationToken cancellationToken)
    {
        return await User.GetUserId()
            .BindAsync(async userId =>
            {
                request.UserId = userId;
                return await _updateLayoutNameService.RenameLayoutAsync(request, cancellationToken);
            })
            .ToActionResultAsync();
    }
}

public interface IUpdateLayoutNameService
{
    public Task<Result<RenameLayoutResultDto, ErrorObject<string>>> RenameLayoutAsync(RenameLayoutRequestDto requestDto,
        CancellationToken cancellationToken = default);
}

public class UpdateLayoutNameService : IUpdateLayoutNameService
{
    private readonly IUpdateLayoutNameRepository _updateLayoutNameRepository;

    public UpdateLayoutNameService(IUpdateLayoutNameRepository updateLayoutNameRepository)
    {
        _updateLayoutNameRepository = updateLayoutNameRepository;
    }

    public async Task<Result<RenameLayoutResultDto, ErrorObject<string>>> RenameLayoutAsync(
        RenameLayoutRequestDto requestDto, CancellationToken cancellationToken = default)
    {
        return await _updateLayoutNameRepository.RenameLayoutAsync(requestDto, cancellationToken);
    }
}

public interface IUpdateLayoutNameRepository
{
    public Task<Result<RenameLayoutResultDto, ErrorObject<string>>> RenameLayoutAsync(RenameLayoutRequestDto requestDto,
        CancellationToken cancellationToken = default);
}

public class UpdateLayoutNameRepository : IUpdateLayoutNameRepository
{
    private readonly ApplicationCommandDbContext _dbContext;

    public UpdateLayoutNameRepository(ApplicationCommandDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<RenameLayoutResultDto, ErrorObject<string>>> RenameLayoutAsync(RenameLayoutRequestDto requestDto,
        CancellationToken cancellationToken = default)
    {
        var rowsChanged = await _dbContext.EditorLayouts.Include(l => l.OwnedBy)
            .Where(l => l.OwnedBy.Select(ol => ol.UserId).Contains(requestDto.UserId) &&
                        l.EditorLayoutId == requestDto.LayoutId && !l.IsDefaultLayout).ExecuteUpdateAsync(
                setters => setters.SetProperty(e => e.LayoutName, requestDto.NewName),
                cancellationToken: cancellationToken);
        if (rowsChanged == 0)
            return Result<RenameLayoutResultDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Could not attribute layout: {requestDto.LayoutId} to user: {requestDto.UserId}"));
        return Result<RenameLayoutResultDto, ErrorObject<string>>.Ok(new RenameLayoutResultDto
        {
            LayoutId = requestDto.LayoutId,
            NewName = requestDto.NewName
        });
    }
}

public class RenameLayoutRequestDto
{
    public required Guid LayoutId { get; init; }
    public required string NewName { get; init; }
    internal Guid UserId { get; set; }
}

public class RenameLayoutResultDto
{
    public required Guid LayoutId { get; init; }
    public required string NewName { get; init; }
}