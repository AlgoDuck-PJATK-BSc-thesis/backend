using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.AdminGetProblemCreatorPreview;

[Route("api/problem/creator/preview")]
[ApiController]
[Authorize(Roles = "admin")]
public class GetProblemCreatorPreviewController : ControllerBase
{
    private readonly IGetProblemCreatorPreviewService _getProblemCreatorPreviewService;

    public GetProblemCreatorPreviewController(IGetProblemCreatorPreviewService getProblemCreatorPreviewService)
    {
        _getProblemCreatorPreviewService = getProblemCreatorPreviewService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProblemCreatorPreviewAsync([FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        return await _getProblemCreatorPreviewService.GetProblemCreatorAsync(userId, cancellationToken).ToActionResultAsync();
    }
}

public interface IGetProblemCreatorPreviewService
{
    public Task<Result<ProblemCreator, ErrorObject<string>>> GetProblemCreatorAsync(Guid userId,
        CancellationToken cancellationToken = default);
}

public class GetProblemCreatorPreviewService : IGetProblemCreatorPreviewService
{
    private readonly IGetProblemCreatorPreviewRepository _getProblemCreatorPreviewRepository;

    public GetProblemCreatorPreviewService(IGetProblemCreatorPreviewRepository getProblemCreatorPreviewRepository)
    {
        _getProblemCreatorPreviewRepository = getProblemCreatorPreviewRepository;
    }

    public async Task<Result<ProblemCreator, ErrorObject<string>>> GetProblemCreatorAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _getProblemCreatorPreviewRepository.GetProblemCreatorAsync(userId, cancellationToken);
    }
}

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

public class ProblemCreator
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required Guid SelectedAvatar { get; init; }
    public required int ProblemCreatedCount { get; init; }
}