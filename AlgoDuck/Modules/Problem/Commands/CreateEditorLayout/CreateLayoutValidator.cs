using FluentValidation;

namespace AlgoDuck.Modules.Problem.Commands.CreateEditorLayout;

public class CreateLayoutValidator : AbstractValidator<LayoutCreateDto>
{
    public CreateLayoutValidator()
    {
        RuleFor(x => x.LayoutContent).NotEmpty().WithMessage("Content is required").MaximumLength(65 * 1024).WithMessage($"Content length must be less than {65 * 1024} bytes");
        RuleFor(x => x.LayoutName).NotEmpty().WithMessage("Name is required").MaximumLength(128).WithMessage($"Name must be less than {128} bytes");
    }
}