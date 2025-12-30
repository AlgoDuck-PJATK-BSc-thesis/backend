using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Commands.EmplacePlantOnHomePage;


public interface IEmplacePlantService
{
    public Task<Result<EmplacePlantResult, ErrorObject<string>>> EmplacePlantAsync(EmplacePlantDto emplacePlantDto, CancellationToken cancellationToken = default);
}


public class EmplacePlantService(
    IEmplacePlantRepository emplacePlantRepository
    ) : IEmplacePlantService
{
    public async Task<Result<EmplacePlantResult, ErrorObject<string>>> EmplacePlantAsync(EmplacePlantDto emplacePlantDto, CancellationToken cancellationToken = default)
    {
        var ownershipResult = await emplacePlantRepository.DoesUserOwnItemAsync(emplacePlantDto, cancellationToken);
        if (ownershipResult.IsErr)
            return Result<EmplacePlantResult, ErrorObject<string>>.Err(ownershipResult.AsT1);
        
        return await emplacePlantRepository.EmplacePlantAsync(emplacePlantDto,  cancellationToken);
    }
}