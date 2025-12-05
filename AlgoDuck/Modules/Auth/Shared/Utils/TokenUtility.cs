using AlgoDuck.Models;

namespace AlgoDuck.Modules.Auth.Shared.Utils;

public sealed class TokenUtility
{
    private const int RefreshPrefixLength = 32;

    public bool ValidateCsrf(string headerValue, string? cookieValue)
    {
        if (string.IsNullOrWhiteSpace(headerValue))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(cookieValue))
        {
            return false;
        }

        return string.Equals(headerValue, cookieValue, StringComparison.Ordinal);
    }

    public string GetRefreshPrefix(string rawRefresh)
    {
        if (string.IsNullOrEmpty(rawRefresh))
        {
            return string.Empty;
        }

        var length = Math.Min(rawRefresh.Length, RefreshPrefixLength);
        return rawRefresh.Substring(0, length);
    }

    public Session GetSessionFromContext(HttpContext context)
    {
        throw new InvalidOperationException("Session resolution from HttpContext is not implemented.");
    }
}