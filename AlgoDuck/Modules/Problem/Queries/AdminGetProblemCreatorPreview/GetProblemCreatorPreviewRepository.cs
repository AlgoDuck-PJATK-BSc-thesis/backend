using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.AdminGetProblemCreatorPreview;

public interface IGetProblemCreatorPreviewRepository
{
    public Task<Result<ProblemCreator, ErrorObject<string>>> GetProblemCreatorAsync(Guid userId,
        CancellationToken cancellationToken = default);
}

public class GetProblemCreatorPreviewRepository : IGetProblemCreatorPreviewRepository
{
    private readonly ApplicationQueryDbContext _dbContext;

    public GetProblemCreatorPreviewRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ProblemCreator, ErrorObject<string>>> GetProblemCreatorAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var problemCreator = await _dbContext.ApplicationUsers. Include(p => p.Purchases).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (problemCreator == null)
            return Result<ProblemCreator, ErrorObject<string>>.Err(
                ErrorObject<string>.NotFound($"User: {userId} was not found"));

        var userItemSelectedAsAvatar = problemCreator.Purchases.OfType<DuckOwnership>()
            .FirstOrDefault(o => o.SelectedAsAvatar);

        var userIconId = userItemSelectedAsAvatar?.ItemId ?? (await _dbContext.DuckItems.FirstAsync(
            d => d.Name == "algoduck", cancellationToken: cancellationToken)).ItemId;
        
        return Result<ProblemCreator, ErrorObject<string>>.Ok(new ProblemCreator
        {
            Email = problemCreator.Email!,
            Id = problemCreator.Id,
            Username = problemCreator.UserName!,
            SelectedAvatar = userIconId,
            ProblemCreatedCount = await _dbContext.Problems.CountAsync(d => d.CreatedByUserId == problemCreator.Id, cancellationToken: cancellationToken),
        });
    }
}
