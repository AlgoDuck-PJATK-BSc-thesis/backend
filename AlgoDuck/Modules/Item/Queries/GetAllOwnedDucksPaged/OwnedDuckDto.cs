namespace AlgoDuck.Modules.Item.Queries.GetAllOwnedDucksPaged;


public class OwnedDuckDto
{
    public required Guid ItemId { get; set; }
    public required bool IsSelectedAsAvatar { get; set; }
    public required bool IsSelectedForPond { get; set; }
}