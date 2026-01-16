using AlgoDuck.Modules.Item.Commands.UpsertItem.CreateItem;
using AlgoDuck.Modules.Item.Queries.AdminGetFullItemDetails.Types;
using AlgoDuck.Shared.Http;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Item.Queries.AdminGetFullItemDetails;

public interface IFullItemDetailsService
{
    public Task<Result<FullItemDetailsDto, ErrorObject<string>>> GetFullItemDetailsAsync(Guid itemId,
        CancellationToken cancellationToken = default);
}

public class FullItemDetailsService : IFullItemDetailsService
{
    private readonly IFullItemDetailsRepository _fullItemDetailsRepository;
    private readonly IOptions<SpriteLegalFileNamesConfiguration> _itemSprites;

    public FullItemDetailsService(IFullItemDetailsRepository fullItemDetailsRepository,
        IOptions<SpriteLegalFileNamesConfiguration> itemSprites)
    {
        _fullItemDetailsRepository = fullItemDetailsRepository;
        _itemSprites = itemSprites;
    }

    public async Task<Result<FullItemDetailsDto, ErrorObject<string>>> GetFullItemDetailsAsync(Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var fullItemDetailsDto = new FullItemDetailsDto();

        return await _fullItemDetailsRepository.GetItemDetailsCore(itemId, cancellationToken).BindAsync(async itemDetails =>
        {
            fullItemDetailsDto.ItemDetailsCore = itemDetails;
            return await _fullItemDetailsRepository.GetTypedObjectDetails(itemDetails.ItemType, itemDetails.ItemId,
                cancellationToken);
        }).BindAsync(async typedDetails =>
        {
            fullItemDetailsDto.ItemTypeSpecificData = typedDetails;
            return await _fullItemDetailsRepository.GetItemStatisticsAsync(itemId,
                fullItemDetailsDto.ItemDetailsCore.ItemType, cancellationToken);
        }).BindAsync(async statistics =>
        {
            fullItemDetailsDto.ItemSpecificStatistics = statistics;
            return await _fullItemDetailsRepository.GetItemPurchaseTimeseriesDataAsync(new TimeSeriesDataRequest()
            {
                Granularity = TimeseriesGranularity.Day,
                ItemId = itemId,
                StartDate = DateTime.UtcNow.AddDays(-7),
            }, cancellationToken);
        }).BindResult(timeseriesData =>
        {
            fullItemDetailsDto.TimeseriesData = timeseriesData;
            fullItemDetailsDto.SpriteList = _itemSprites.Value.LegalFileNames.TryGetValue(fullItemDetailsDto.ItemDetailsCore.ItemType, out var sprites) ? sprites : [];
            return Result<FullItemDetailsDto, ErrorObject<string>>.Ok(fullItemDetailsDto);
        });
    }
}