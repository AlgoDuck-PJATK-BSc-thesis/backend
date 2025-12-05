using FluentValidation;

namespace AlgoDuck.Modules.Auth.Queries.CheckUserPermissions;

public sealed class CheckUserPermissionsValidator : AbstractValidator<PermissionsDto>
{
    public CheckUserPermissionsValidator()
    {
        RuleFor(x => x.Permissions)
            .NotNull()
            .Must(p => p.Count > 0)
            .WithMessage("At least one permission must be provided.");
    }
}