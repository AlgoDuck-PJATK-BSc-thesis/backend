namespace AlgoDuck.Modules.Item.Queries.AdminGetFullItemDetails.Types;

public class TimeSeriesDataRequest
{
    public required Guid ItemId { get; set; }
    public required TimeseriesGranularity Granularity { get; set; }
    public required DateTime StartDate { get; set; }
}