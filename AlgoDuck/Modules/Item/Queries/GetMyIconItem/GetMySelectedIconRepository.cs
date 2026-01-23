using AlgoDuck.DAL;
using AlgoDuck.Shared.Result;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetMyIconItem;

public interface IGetMySelectedIconRepository
{
    public Task<Result<MySelectedIconDto, NotFoundError<string>>> GetMySelectedIconAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class GetMySelectedIconRepository : IGetMySelectedIconRepository
{
    private readonly ApplicationQueryDbContext _dbContext;

    public GetMySelectedIconRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<MySelectedIconDto, NotFoundError<string>>> GetMySelectedIconAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var res = await _dbContext.DuckOwnerships.Include(dow => dow.Item).Where(dow => dow.UserId == userId && dow.SelectedAsAvatar)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return res == null
            ? Result<MySelectedIconDto, NotFoundError<string>>.Err(new NotFoundError<string>("Could not resolve selected avatar"))
            : Result<MySelectedIconDto, NotFoundError<string>>.Ok(new MySelectedIconDto
            {
                ItemId = res.ItemId,
                ItemName = res.Item.Name
            }); 
    }
}
