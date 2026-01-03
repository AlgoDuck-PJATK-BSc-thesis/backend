using FluentValidation;

namespace AlgoDuck.Modules.User.Commands.AdminUpdateUser;

public sealed class AdminUpdateUserValidator : AbstractValidator<AdminUpdateUserDto>
{
    public AdminUpdateUserValidator()
    {
        RuleFor(x => x).Must(x =>
        {
            var hasUsername = x.Username is not null && !string.IsNullOrWhiteSpace(x.Username);
            var hasRole = x.Role is not null && !string.IsNullOrWhiteSpace(x.Role);
            return hasUsername || hasRole;
        });

        When(x => x.Username is not null, () =>
        {
            RuleFor(x => x.Username!)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(64);
        });

        When(x => x.Role is not null, () =>
        {
            RuleFor(x => x.Role!)
                .NotEmpty()
                .Must(r =>
                {
                    var v = r.Trim().ToLowerInvariant();
                    return v == "user" || v == "admin";
                });
        });
    }
}