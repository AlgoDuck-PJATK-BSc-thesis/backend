using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AlgoDuck.Modules.Item.Commands.CreateItem.Types;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(DuckData), "duck")]
[JsonDerivedType(typeof(PlantData), "plant")]
public interface IItemTypeSpecificData;

public class DuckData : IItemTypeSpecificData;

public class PlantData : IItemTypeSpecificData
{
    [Range(minimum: 0, maximum: byte.MaxValue)]
    public required byte Width { get; set; }
    [Range(minimum: 0, maximum: byte.MaxValue)]
    public required byte Height { get; set; }
}
