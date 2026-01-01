using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetCustomLayoutDetails;


public interface ICustomLayoutDetailsService
{
    public Task<Result<CustomLayoutDetailsResponseDto, ErrorObject<string>>> GetCustomLayoutDetailsASync(CustomLayoutDetailsRequestDto requestDto, CancellationToken cancellationToken = default);
}

public class CustomLayoutDetailsService(
    ICustomLayoutDetailsRepository layoutDetailsRepository
    ) : ICustomLayoutDetailsService
{
    public async Task<Result<CustomLayoutDetailsResponseDto, ErrorObject<string>>> GetCustomLayoutDetailsASync(CustomLayoutDetailsRequestDto requestDto,
        CancellationToken cancellationToken = default)
    {
        return await layoutDetailsRepository.GetCustomLayoutDetailsASync(requestDto, cancellationToken);
    }
}