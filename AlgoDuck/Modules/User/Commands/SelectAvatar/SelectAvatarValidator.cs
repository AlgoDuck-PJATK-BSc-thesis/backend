using FluentValidation;

namespace AlgoDuck.Modules.User.Commands.SelectAvatar;

public sealed class SelectAvatarValidator : AbstractValidator<SelectAvatarDto>
{
    public SelectAvatarValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
    }
}