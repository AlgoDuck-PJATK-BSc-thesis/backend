using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;

namespace AlgoDuck.Modules.Problem.Queries.GetCustomLayoutDetails;

public interface ICustomLayoutDetailsRepository
{
    public Task<Result<CustomLayoutDetailsResponseDto, ErrorObject<string>>> GetCustomLayoutDetailsASync(CustomLayoutDetailsRequestDto requestDto, CancellationToken cancellationToken = default);
}

public class CustomLayoutDetailsRepository(
    IAwsS3Client s3Client
    ) : ICustomLayoutDetailsRepository
{
    public async Task<Result<CustomLayoutDetailsResponseDto, ErrorObject<string>>> GetCustomLayoutDetailsASync(CustomLayoutDetailsRequestDto requestDto,
        CancellationToken cancellationToken = default)
    {
        var layoutPath = $"users/{requestDto.UserId}/layouts/{requestDto.LayoutId}.json";
        var documentStringResult = await s3Client.GetJsonObjectByPathAsync<object>(path: layoutPath, cancellationToken: cancellationToken);
        
        if (documentStringResult.IsErr)
            return Result<CustomLayoutDetailsResponseDto, ErrorObject<string>>.Err(documentStringResult.AsErr!);
        
        return Result<CustomLayoutDetailsResponseDto, ErrorObject<string>>.Ok(new CustomLayoutDetailsResponseDto
        {
            LayoutId = requestDto.LayoutId,
            LayoutContents = documentStringResult.AsOk!,
        });
    }
}