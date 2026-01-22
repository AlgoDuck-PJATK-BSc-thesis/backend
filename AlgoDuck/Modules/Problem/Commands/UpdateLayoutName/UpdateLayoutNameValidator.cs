using FluentValidation;

namespace AlgoDuck.Modules.Problem.Commands.UpdateLayoutName;

public class UpdateLayoutNameValidator : AbstractValidator<RenameLayoutRequestDto>
{
    public UpdateLayoutNameValidator()
    {
        RuleFor(x => x.NewName).NotEmpty().WithMessage("Request must contain new name").MaximumLength(256).WithMessage("Layout name may not be greater than 256 characters");
    }
}