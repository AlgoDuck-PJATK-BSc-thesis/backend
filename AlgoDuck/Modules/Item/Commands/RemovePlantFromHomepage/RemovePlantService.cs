using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Commands.RemovePlantFromHomepage;

public interface IRemovePlantService
{
    public Task<Result<Guid, ErrorObject<string>>> RemovePlantFromHomepageAsync(RemovePlantDto removePlantDto, CancellationToken cancellationToken = default);
    
}

public class RemovePlantService(
    IRemovePlantRepository removePlantRepository
    ) : IRemovePlantService
{
    public async Task<Result<Guid, ErrorObject<string>>> RemovePlantFromHomepageAsync(RemovePlantDto removePlantDto, CancellationToken cancellationToken = default)
    {
        return await removePlantRepository.RemovePlantFromHomepageAsync(removePlantDto, cancellationToken);    
    }
}