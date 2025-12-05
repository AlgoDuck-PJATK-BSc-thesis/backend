using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.EnableTwoFactor;

public sealed class EnableTwoFactorValidator : AbstractValidator<EnableTwoFactorDto>
{
    public EnableTwoFactorValidator()
    {
    }
}