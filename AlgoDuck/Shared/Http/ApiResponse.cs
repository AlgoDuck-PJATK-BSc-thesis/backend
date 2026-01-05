using System.Text.Json.Serialization;

namespace AlgoDuck.Shared.Http;

public interface IApiResponse;

internal class StandardApiResponse<T> : IApiResponse
{
    [JsonPropertyOrder(0)]
    public Status Status { get; set; } = Status.Success;

    [JsonPropertyOrder(1)]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyOrder(2)]
    public T? Body { get; set; }
}

internal class StandardApiResponse : IApiResponse
{
    [JsonPropertyOrder(0)]
    public Status Status { get; set; } = Status.Success;

    [JsonPropertyOrder(1)]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyOrder(2)]
    public object? Body { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
    Success,
    Warning,
    Error
}