using System.Text.Json.Serialization;

namespace AlgoDuck.Shared.Http;

public record ApiResponse(string status, object? data, string? message, string? code)
{
    public static ApiResponse Success(object? data = null) => new("success", data, null, null);
    public static ApiResponse Fail(string message, string? code = null, object? data = null) => new("fail", data, message, code);
    public static ApiResponse Error(string message, string? code = null) => new("error", null, message, code);
}

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
