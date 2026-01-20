using FluentValidation;

namespace AlgoDuck.Modules.User.Commands.User.Profile.SelectAvatar;

public sealed class SelectAvatarValidator : AbstractValidator<SelectAvatarDto>
{
    public SelectAvatarValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
    }
}