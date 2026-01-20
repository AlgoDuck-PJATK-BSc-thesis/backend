namespace AlgoDuck.Modules.Item.Queries.AdminGetAllItemsPagedFilterable;


public class ItemDto
{
    public required Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public DateTime? CreatedOn { get; set; }
    public Guid? CreatedBy { get; set; }
    public int? OwnedCount { get; set; }
    public string? Type { get; set; }
}