using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetCustomLayoutDetails;

public interface ICustomLayoutDetailsRepository
{
    public Task<Result<CustomLayoutDetailsResponseDto, ErrorObject<string>>> GetCustomLayoutDetailsAsync(
        CustomLayoutDetailsRequestDto requestDto, CancellationToken cancellationToken = default);
}

public class CustomLayoutDetailsRepository : ICustomLayoutDetailsRepository
{
    private readonly IAwsS3Client _s3Client;
    private readonly ApplicationCommandDbContext _dbContext;

    public CustomLayoutDetailsRepository(IAwsS3Client s3Client, ApplicationCommandDbContext dbContext)
    {
        _s3Client = s3Client;
        _dbContext = dbContext;
    }

    public async Task<Result<CustomLayoutDetailsResponseDto, ErrorObject<string>>> GetCustomLayoutDetailsAsync(
        CustomLayoutDetailsRequestDto requestDto,
        CancellationToken cancellationToken = default)
    {
        var layoutDbFetchTask = _dbContext.OwnsLayouts.Include(ol => ol.Layout)
            .Where(ol => ol.UserId == requestDto.UserId && ol.LayoutId == requestDto.LayoutId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        var layoutPath = $"users/layouts/{requestDto.LayoutId}.json";

        var layoutS3FetchTask = _s3Client
            .GetJsonObjectByPathAsync<object>(path: layoutPath, cancellationToken: cancellationToken);

        await Task.WhenAll(layoutS3FetchTask, layoutDbFetchTask);
        
        if (layoutDbFetchTask.Result == null)
            return Result<CustomLayoutDetailsResponseDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest($"Could not attribute layout with id: {requestDto.LayoutId} to user: {requestDto.UserId}"));
        
        if (layoutS3FetchTask.Result.IsErr)
            return Result<CustomLayoutDetailsResponseDto, ErrorObject<string>>.Err(layoutS3FetchTask.Result.AsErr!);

        await _dbContext.OwnsLayouts
            .Where(l => l.UserId == requestDto.UserId && l.IsSelected)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.IsSelected, false), cancellationToken: cancellationToken);
                
        layoutDbFetchTask.Result.IsSelected = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return Result<CustomLayoutDetailsResponseDto, ErrorObject<string>>.Ok(new CustomLayoutDetailsResponseDto()
        {
            LayoutId = requestDto.LayoutId,
            LayoutContent = layoutS3FetchTask.Result.AsOk!,
            LayoutName = layoutDbFetchTask.Result.Layout.LayoutName
        });
    }
}