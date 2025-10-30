namespace AlgoDuck.Shared.Http;

public record ApiResponse(string status, object? data, string? message, string? code)
{
    public static ApiResponse Success(object? data = null) => new("success", data, null, null);
    public static ApiResponse Fail(string message, string? code = null, object? data = null) => new("fail", data, message, code);
    public static ApiResponse Error(string message, string? code = null) => new("error", null, message, code);
}