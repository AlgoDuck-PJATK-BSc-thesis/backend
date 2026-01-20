using FluentValidation;

namespace AlgoDuck.Modules.User.Commands.Admin.UpdateUser;

public sealed class UpdateUserValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x).Must(x =>
        {
            var hasUsername = x.Username is not null && !string.IsNullOrWhiteSpace(x.Username);
            var hasRole = x.Role is not null && !string.IsNullOrWhiteSpace(x.Role);
            var hasEmail = x.Email is not null && !string.IsNullOrWhiteSpace(x.Email);
            var hasPassword = x.Password is not null && !string.IsNullOrWhiteSpace(x.Password);
            return hasUsername || hasRole || hasEmail || hasPassword;
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

        When(x => x.Email is not null, () =>
        {
            RuleFor(x => x.Email!)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(256);
        });

        When(x => x.Password is not null, () =>
        {
            RuleFor(x => x.Password!)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(128);
        });
    }
}