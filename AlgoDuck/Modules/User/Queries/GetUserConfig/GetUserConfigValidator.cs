using FluentValidation;

namespace AlgoDuck.Modules.User.Queries.GetUserConfig;

public sealed class GetUserConfigValidator : AbstractValidator<GetUserConfigQuery>
{
    public GetUserConfigValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}