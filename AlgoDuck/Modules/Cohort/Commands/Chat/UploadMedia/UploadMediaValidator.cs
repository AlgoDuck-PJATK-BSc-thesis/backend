using AlgoDuck.Modules.Cohort.Shared.Utils;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Cohort.Commands.Chat.UploadMedia;

public sealed class UploadMediaValidator : AbstractValidator<UploadMediaDto>
{
    public UploadMediaValidator(IOptions<ChatMediaSettings> mediaOptions)
    {
        var settings = mediaOptions.Value;

        RuleFor(x => x.CohortId)
            .NotEmpty();

        RuleFor(x => x.File)
            .NotNull()
            .DependentRules(() =>
            {
                RuleFor(x => x.File)
                    .Must(f => f != null && f.Length > 0)
                    .WithMessage("File cannot be empty.")
                    .Must(f => f != null && f.Length <= settings.MaxFileSizeBytes)
                    .WithMessage("File exceeds maximum allowed size.")
                    .Must(f => f != null && settings.AllowedContentTypes.Contains(f.ContentType, StringComparer.OrdinalIgnoreCase))
                    .WithMessage("File content type is not allowed.");
            });
    }
}