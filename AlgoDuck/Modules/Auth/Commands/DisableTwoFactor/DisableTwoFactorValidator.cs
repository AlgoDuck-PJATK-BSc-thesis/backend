using FluentValidation;

namespace AlgoDuck.Modules.Auth.Commands.DisableTwoFactor;

public sealed class DisableTwoFactorValidator : AbstractValidator<DisableTwoFactorDto>
{
    public DisableTwoFactorValidator()
    {
    }
}