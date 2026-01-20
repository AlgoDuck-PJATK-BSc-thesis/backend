using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetUserPreviewData;

[Authorize(Roles = "admin")]
[Route("api/user/item")]
[ApiController]
public class UserPreviewDataController
{
    private readonly IGetUserPreviewService _getUserPreviewService;

    public UserPreviewDataController(IGetUserPreviewService getUserPreviewService)
    {
        _getUserPreviewService = getUserPreviewService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserPreviewAsync([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        return await _getUserPreviewService.GetUserPreviewAsync(userId, cancellationToken).ToActionResultAsync();
    }
}

public interface IGetUserPreviewService
{
    public Task<Result<UserPreviewDto, ErrorObject<string>>> GetUserPreviewAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class GetUserPreviewService : IGetUserPreviewService
{
    private readonly IGetUserPreviewRepository _getUserPreviewRepository;

    public GetUserPreviewService(IGetUserPreviewRepository getUserPreviewRepository)
    {
        _getUserPreviewRepository = getUserPreviewRepository;
    }

    public async Task<Result<UserPreviewDto, ErrorObject<string>>> GetUserPreviewAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _getUserPreviewRepository.GetUserPreviewAsync(userId, cancellationToken);
    }
}

public interface IGetUserPreviewRepository
{
    public Task<Result<UserPreviewDto, ErrorObject<string>>> GetUserPreviewAsync(Guid userId, CancellationToken cancellationToken = default);
    
}

public class GetUserPreviewRepository : IGetUserPreviewRepository
{
    
    private readonly ApplicationQueryDbContext _dbContext;

    public GetUserPreviewRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserPreviewDto, ErrorObject<string>>> GetUserPreviewAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.ApplicationUsers
            .Include(u => u.Purchases)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
            return Result<UserPreviewDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"User {userId} not found"));

        var userSelectedDuck = user.Purchases.Where(p => p is DuckOwnership)
            .FirstOrDefault(ow => (ow as DuckOwnership)!.SelectedAsAvatar);

        var userIconId = userSelectedDuck?.ItemId;
        if (userIconId == null)
        {
            var defaultDuck = await _dbContext.DuckItems.FirstAsync(d => d.Name == "algoduck", cancellationToken: cancellationToken);
            userIconId = defaultDuck.ItemId;
        }
        
        return Result<UserPreviewDto, ErrorObject<string>>.Ok(new UserPreviewDto
        {
            Id = user.Id,
            SelectedAvatar = (Guid) userIconId,
            Username = user.UserName!,
            Email = user.Email!,
            ItemCreatedCount = await _dbContext.Items.CountAsync(i => i.CreatedById == userId, cancellationToken)
        });
    }
}

public class UserPreviewDto
{
    public required Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required Guid SelectedAvatar { get; set; }
    public required long ItemCreatedCount { get; set; }
}