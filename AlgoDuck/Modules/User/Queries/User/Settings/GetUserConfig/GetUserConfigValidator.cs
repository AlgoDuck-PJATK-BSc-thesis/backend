using FluentValidation;

namespace AlgoDuck.Modules.User.Queries.User.Settings.GetUserConfig;

public sealed class GetUserConfigValidator : AbstractValidator<GetUserConfigRequestDto>
{
    public GetUserConfigValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}