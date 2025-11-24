using System.Text.Json.Serialization;

namespace AlgoDuck.Shared.Http;

internal class StandardApiResponse<T>
{
    public Status Status { get; set; } = Status.Success;
    public T? Body { get; set; }
    public string Message { get; set; } = string.Empty;
}

internal class StandardApiResponse
{
    public Status Status { get; set; } = Status.Success;
    public string Message { get; set; } = string.Empty;
}

[JsonConverter(typeof(JsonStringEnumConverter))]
internal enum Status
{
    Success,
    Warning,
    Error
}
