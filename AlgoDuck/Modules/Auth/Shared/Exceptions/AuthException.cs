using AlgoDuck.Shared.Exceptions;

namespace AlgoDuck.Modules.Auth.Shared.Exceptions;

public abstract class AuthException : AppException
{
    public string Code { get; }

    protected AuthException(string code, string message)
        : base(message, MapStatusCode(code, message))
    {
        Code = code;
    }

    protected AuthException(string code, string message, Exception? innerException)
        : base(message, MapStatusCode(code, message), innerException ?? new Exception(message))
    {
        Code = code;
    }

    private static int MapStatusCode(string code, string message)
    {
        if (string.Equals(code, "permission_denied", StringComparison.OrdinalIgnoreCase))
            return StatusCodes.Status403Forbidden;

        if (!string.IsNullOrWhiteSpace(message))
        {
            var m = message.ToLowerInvariant();

            if (m.Contains("not found"))
                return StatusCodes.Status404NotFound;

            if (m.Contains("not authenticated") || m.Contains("unauthenticated") || m.Contains("unauthorized"))
                return StatusCodes.Status401Unauthorized;

            if (m.Contains("invalid"))
                return StatusCodes.Status400BadRequest;
        }

        return StatusCodes.Status401Unauthorized;
    }
}
