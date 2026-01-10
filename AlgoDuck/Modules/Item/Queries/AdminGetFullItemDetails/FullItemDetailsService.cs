using AlgoDuck.Modules.Item.Queries.AdminGetFullItemDetails;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetFullItemDetails;

public interface IFullItemDetailsService
{
    public Task<Result<FullItemDetailsDto, ErrorObject<string>>> GetFullItemDetailsAsync(Guid itemId, CancellationToken cancellationToken = default);
}

public class FullItemDetailsService(
    IFullItemDetailsRepository fullItemDetailsRepository
) : IFullItemDetailsService
{
    public async Task<Result<FullItemDetailsDto, ErrorObject<string>>> GetFullItemDetailsAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await fullItemDetailsRepository.GetFullItemDetailsAsync(itemId, cancellationToken);
    }
}