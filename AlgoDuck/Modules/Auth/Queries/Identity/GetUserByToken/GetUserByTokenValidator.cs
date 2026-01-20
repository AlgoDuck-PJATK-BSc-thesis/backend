using FluentValidation;

namespace AlgoDuck.Modules.Auth.Queries.Identity.GetUserByToken;

public sealed class GetUserByTokenValidator : AbstractValidator<UserByTokenDto>
{
    public GetUserByTokenValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty();
    }
}