using System.ComponentModel.DataAnnotations;
using AlgoDuck.Modules.Item.Commands.CreateItem;
using AlgoDuck.Modules.Item.Commands.CreateItem.Types;
using AlgoDuck.Shared.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Commands.UpsertItem.CreateItem.Types;

public class CreateItemRequestDto
{

    [MaxLength(128)]
    public required string ItemName { get; set; }
    [MaxLength(128)]
    public string? Description { get; set; } 
    [Range(minimum: 0, maximum: int.MaxValue)]
    public required int ItemCost { get; set; }
    public required Guid RarityId { get; set; }
    [ModelBinder(BinderType = typeof(ItemDataModelBinder))]
    public required IItemTypeSpecificData ItemData { get; set; }
    [FromForm] 
    [MaxFileCount(10)] 
    [MaxFileSize(512 * 1024)]
    public required IFormFileCollection Sprites { get; set; }
    internal Guid CreatedByUserId { get; set; }
    
}
