using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetValidator : AbstractValidator<RequestPasswordResetDto>
{
    public RequestPasswordResetValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(256)
            .EmailAddress();
    }
}