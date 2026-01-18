using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetMyIconItem;

[ApiController]
[Authorize]
[Route("api/item/avatar")]
public class MyItemController : ControllerBase
{
    private readonly IGetMySelectedIconService _getMySelectedIconService;

    public MyItemController(IGetMySelectedIconService getMySelectedIconService)
    {
        _getMySelectedIconService = getMySelectedIconService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMySelectedIconAsync(CancellationToken cancellationToken)
    {
        return await User.GetUserId()
            .BindAsync(async userId =>
                await _getMySelectedIconService.GetMySelectedIconAsync(userId, cancellationToken))
            .ToActionResultAsync();
    }
}

public interface IGetMySelectedIconService
{
    public Task<Result<MySelectedIconDto, ErrorObject<string>>> GetMySelectedIconAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class GetMySelectedIconService : IGetMySelectedIconService
{
    private readonly IGetMySelectedIconRepository _getMySelectedIconRepository;

    public GetMySelectedIconService(IGetMySelectedIconRepository getMySelectedIconRepository)
    {
        _getMySelectedIconRepository = getMySelectedIconRepository;
    }

    public async Task<Result<MySelectedIconDto, ErrorObject<string>>> GetMySelectedIconAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _getMySelectedIconRepository.GetMySelectedIconAsync(userId, cancellationToken);
    }
}

public interface IGetMySelectedIconRepository
{
    public Task<Result<MySelectedIconDto, ErrorObject<string>>> GetMySelectedIconAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class GetMySelectedIconRepository : IGetMySelectedIconRepository
{
    private readonly ApplicationQueryDbContext _dbContext;

    public GetMySelectedIconRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<MySelectedIconDto, ErrorObject<string>>> GetMySelectedIconAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var res = await _dbContext.DuckOwnerships.Include(dow => dow.Item).Where(dow => dow.UserId == userId && dow.SelectedAsAvatar)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return res == null
            ? Result<MySelectedIconDto, ErrorObject<string>>.Err(
                ErrorObject<string>.NotFound("Could not resolve selected avatar"))
            : Result<MySelectedIconDto, ErrorObject<string>>.Ok(new MySelectedIconDto
            {
                ItemId = res.ItemId,
                ItemName = res.Item.Name
            }); 
    }
}

public class MySelectedIconDto
{
    public required Guid ItemId { get; set; }
    public required string ItemName { get; set; }
}