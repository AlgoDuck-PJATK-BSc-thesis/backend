namespace AlgoDuck.Modules.Cohort.Shared.Utils;

using System.Text.Json.Serialization;

internal sealed class ModerationApiRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;
}

internal sealed class ModerationApiResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("results")]
    public List<ModerationResult>? Results { get; set; }
}

internal sealed class ModerationResult
{
    [JsonPropertyName("flagged")]
    public bool Flagged { get; set; }

    [JsonPropertyName("categories")]
    public Dictionary<string, bool> Categories { get; set; } = new();

    [JsonPropertyName("category_scores")]
    public Dictionary<string, double> CategoryScores { get; set; } = new();
}

