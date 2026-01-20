using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetUserPreviewData;

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
