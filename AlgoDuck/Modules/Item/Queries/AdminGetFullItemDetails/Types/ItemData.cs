using System.Text.Json.Serialization;

namespace AlgoDuck.Modules.Item.Queries.AdminGetFullItemDetails.Types;

[JsonDerivedType(typeof(DuckData), "Duck")]
[JsonDerivedType(typeof(PlantData), "Plant")]
public interface IItemTypeSpecificData;

public class DuckData : IItemTypeSpecificData;

public class PlantData : IItemTypeSpecificData
{
    public required byte Width { get; set; }
    public required byte Height { get; set; }
}