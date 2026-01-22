using FluentValidation;

namespace AlgoDuck.Modules.Problem.Commands.UpdateChatName;

public class UpdateChatNameValidator : AbstractValidator<UpdateChatNameDto>
{
    public UpdateChatNameValidator()
    {
        RuleFor(x => x.NewChatName).NotEmpty().WithMessage("Request must contain new chat name").MaximumLength(256).WithMessage("Chat name may not be greater than 256 characters");
    }
}