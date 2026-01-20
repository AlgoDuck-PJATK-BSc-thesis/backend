using System.Text.Json.Serialization;

namespace AlgoDuck.Modules.Item.Queries.AdminGetFullItemDetails.Types;

[JsonDerivedType(typeof(DuckOwnershipStatistics), "Duck")]
[JsonDerivedType(typeof(PlantOwnershipStatistics), "Plant")]
public abstract class ItemSpecificStatistics
{
    public required long OwnedByCount { get; set; }
    public required double OwnedByPercentageOfPopulation { get; set; }
    public required long UsedByCount { get; set; }
    public required double UsedByPercentageOfPopulation { get; set; }
}

public class DuckOwnershipStatistics : ItemSpecificStatistics
{
    public required long UsedAsAvatar { get; set; }
    public required double UsedAsAvatarPercentageOfPopulation { get; set; }
    public required long UsedForPond { get; set; }
    public required double UsedForPondPercentageOfPopulation { get; set; }
}

public class PlantOwnershipStatistics : ItemSpecificStatistics
{
    public required long UsedForPond { get; set; }
    public required double UsedForPondPercentageOfPopulation { get; set; }
}