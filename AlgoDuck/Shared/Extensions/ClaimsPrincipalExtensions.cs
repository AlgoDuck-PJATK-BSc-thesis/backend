using System.Security.Claims;
using AlgoDuck.Shared.Exceptions;

namespace AlgoDuck.Shared.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var idValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idValue))
            throw new UnauthorizedException("User ID claim is missing");

        if (!Guid.TryParse(idValue, out var id))
            throw new UnauthorizedException("User ID claim is invalid");

        return id;
    }
}