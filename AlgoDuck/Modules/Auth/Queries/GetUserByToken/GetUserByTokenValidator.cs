using FluentValidation;

namespace AlgoDuck.Modules.Auth.Queries.GetUserByToken;

public sealed class GetUserByTokenValidator : AbstractValidator<UserByTokenDto>
{
    public GetUserByTokenValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty();
    }
}