using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetCustomLayoutDetails;


public interface ICustomLayoutDetailsService
{
    public Task<Result<CustomLayoutDetailsResponseDto, ErrorObject<string>>> SetCustomLayoutDetailsASync(CustomLayoutDetailsRequestDto requestDto, CancellationToken cancellationToken = default);
}

public class CustomLayoutDetailsService : ICustomLayoutDetailsService
{
    private readonly ICustomLayoutDetailsRepository _layoutDetailsRepository;

    public CustomLayoutDetailsService(ICustomLayoutDetailsRepository layoutDetailsRepository)
    {
        _layoutDetailsRepository = layoutDetailsRepository;
    }

    public async Task<Result<CustomLayoutDetailsResponseDto, ErrorObject<string>>> SetCustomLayoutDetailsASync(CustomLayoutDetailsRequestDto requestDto,
        CancellationToken cancellationToken = default)
    {
        return await _layoutDetailsRepository.GetCustomLayoutDetailsAsync(requestDto, cancellationToken);
    }
}