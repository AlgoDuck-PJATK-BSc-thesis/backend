namespace AlgoDuck.Modules.Item.Commands.CreateItem.Types;

public class ItemCreateResponseDto
{
    public required Guid CreatedItemGuid { get; set; }
    public required ICollection<FilePostingResult> Files { get; set; }
}