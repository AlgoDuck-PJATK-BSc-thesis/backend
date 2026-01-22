using System.Security.Claims;

namespace AlgoDuck.Shared.Http.Result2;

public static class ClaimsPrincipalExtension
{
    public static Result<Guid, ErrorUnion<NotFoundError<string>, ValidationError<string>>> UserIdToResult(this ClaimsPrincipal claimsPrincipal)
    {
        var findFirstValue = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(findFirstValue))
        {
            return Result<Guid, ErrorUnion<NotFoundError<string>, ValidationError<string>>>.Err(new NotFoundError<string>("User Id not found"));
        }
        try
        {
            return Result<Guid, ErrorUnion<NotFoundError<string>, ValidationError<string>>>.Ok(Guid.Parse(findFirstValue));
        }
        catch (FormatException)
        {
            return Result<Guid, ErrorUnion<NotFoundError<string>, ValidationError<string>>>.Err(new ValidationError<string>("User Id is not a valid GUID"));
        }
    }
}