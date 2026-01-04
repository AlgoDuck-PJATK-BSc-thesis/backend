using AlgoDuck.Modules.Cohort.Shared.Utils;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.Chat.SendMessage;

public sealed class SendMessageValidator : AbstractValidator<SendMessageDto>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.CohortId)
            .NotEmpty();

        When(x => x.MediaType == ChatMediaType.Text, () =>
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .MaximumLength(ChatConstants.MaxMessageLength);
        });

        When(x => x.MediaType == ChatMediaType.Image, () =>
        {
            RuleFor(x => x.MediaKey)
                .NotEmpty();
            RuleFor(x => x.MediaContentType)
                .NotEmpty();
        });
    }
}