namespace AlgoDuck.Modules.Item.Queries.AdminGetFullItemDetails.Types;

public class FullItemDetailsDto
{

    public ItemDetailsCore ItemDetailsCore { get; set; } = null!;
    public ICollection<string> SpriteList { get; set; } = [];
    public IItemTypeSpecificData ItemTypeSpecificData { get; set; } = null!;
    public ItemSpecificStatistics ItemSpecificStatistics { get; set; } = null!;
    public ItemPurchaseTimeseriesData TimeseriesData { get; set; } = null!;
}
