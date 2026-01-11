using System.Text.Json.Serialization;

namespace AlgoDuck.Modules.Item.Queries.AdminGetFullItemDetails.Types;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TimeseriesGranularity
{
    Month, 
    Day
}

public class ItemPurchaseTimeseriesData
{
    public required TimeseriesGranularity Granularity { get; set; }
    public required DateTime StartDate { get; set; }
    public required ICollection<TimeseriesBucket> Buckets { get; set; }
}

public class TimeseriesBucket
{
    public required string Label { get; set; }
    public required long Value { get; set; }
}