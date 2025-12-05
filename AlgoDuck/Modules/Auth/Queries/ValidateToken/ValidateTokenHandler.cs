using System.Security.Claims;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace AlgoDuck.Modules.Auth.Queries.ValidateToken;

public sealed class ValidateTokenHandler : IValidateTokenHandler
{
    private readonly JwtTokenProvider _jwtTokenProvider;

    public ValidateTokenHandler(JwtTokenProvider jwtTokenProvider)
    {
        _jwtTokenProvider = jwtTokenProvider;
    }

    public Task<ValidateTokenResult> HandleAsync(ValidateTokenDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.AccessToken))
        {
            return Task.FromResult(new ValidateTokenResult
            {
                IsValid = false,
                IsExpired = false
            });
        }

        try
        {
            var principal = _jwtTokenProvider.ValidateToken(dto.AccessToken);

            var userId = GetGuidClaim(principal, ClaimTypes.NameIdentifier) ?? GetGuidClaim(principal, "sub");
            var sessionId = GetGuidClaim(principal, "session_id");
            var expiresAt = GetExpiresAt(principal);

            var result = new ValidateTokenResult
            {
                IsValid = true,
                IsExpired = expiresAt.HasValue && expiresAt.Value <= DateTimeOffset.UtcNow,
                UserId = userId,
                SessionId = sessionId,
                ExpiresAt = expiresAt
            };

            return Task.FromResult(result);
        }
        catch (SecurityTokenExpiredException)
        {
            return Task.FromResult(new ValidateTokenResult
            {
                IsValid = false,
                IsExpired = true
            });
        }
        catch
        {
            return Task.FromResult(new ValidateTokenResult
            {
                IsValid = false,
                IsExpired = false
            });
        }
    }

    private static Guid? GetGuidClaim(ClaimsPrincipal principal, string type)
    {
        var claim = principal.FindFirst(type);
        if (claim is null)
        {
            return null;
        }

        return Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    private static DateTimeOffset? GetExpiresAt(ClaimsPrincipal principal)
    {
        var expClaim = principal.FindFirst("exp");
        if (expClaim is null)
        {
            return null;
        }

        if (!long.TryParse(expClaim.Value, out var seconds))
        {
            return null;
        }

        var epoch = DateTimeOffset.FromUnixTimeSeconds(seconds);
        return epoch;
    }
}