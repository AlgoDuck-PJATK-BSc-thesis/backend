using System.Security.Claims;

namespace AlgoDuck.Shared.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var idValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idValue))
        {
            throw new InvalidOperationException(("User ID claim is missing"));
        }
        return Guid.Parse(idValue);
    }
}