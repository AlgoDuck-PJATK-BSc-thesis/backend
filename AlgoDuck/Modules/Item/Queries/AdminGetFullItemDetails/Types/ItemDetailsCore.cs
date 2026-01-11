namespace AlgoDuck.Modules.Item.Queries.AdminGetFullItemDetails.Types;

public class ItemDetailsCore
{
    public required Guid ItemId { get; set; }
    public required string ItemName { get; set; }
    public string? ItemDescription { get; set; }
    public required DateTime CreatedOn { get; set; }
    public required Guid CreatedBy { get; set; }
    public required int Purchases { get; set; }
    public required int Price { get; set; }
    public required string ItemType { get; set; }
}