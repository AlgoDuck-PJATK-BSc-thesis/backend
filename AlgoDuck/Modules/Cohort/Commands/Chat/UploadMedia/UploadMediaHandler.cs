using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.Chat.UploadMedia;

public sealed class UploadMediaHandler : IUploadMediaHandler
{
    private const long MaxImageBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    private readonly IValidator<UploadMediaDto> _validator;
    private readonly ICohortRepository _cohortRepository;
    private readonly IChatMediaStorageService _chatMediaStorageService;

    public UploadMediaHandler(
        IValidator<UploadMediaDto> validator,
        ICohortRepository cohortRepository,
        IChatMediaStorageService chatMediaStorageService)
    {
        _validator = validator;
        _cohortRepository = cohortRepository;
        _chatMediaStorageService = chatMediaStorageService;
    }

    public async Task<UploadMediaResultDto> HandleAsync(
        Guid userId,
        UploadMediaDto dto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new CohortValidationException("Invalid media upload request.");
        }

        var belongs = await _cohortRepository.UserBelongsToCohortAsync(userId, dto.CohortId, cancellationToken);
        if (!belongs)
        {
            throw new CohortValidationException("User does not belong to this cohort.");
        }

        if (dto.File.Length <= 0)
        {
            throw new CohortValidationException("File is empty.");
        }

        if (dto.File.Length > MaxImageBytes)
        {
            throw new CohortValidationException("File is too large.");
        }

        var contentType = (dto.File.ContentType).Trim();
        if (!AllowedContentTypes.Contains(contentType))
        {
            throw new CohortValidationException("Unsupported media type.");
        }

        var descriptor = await _chatMediaStorageService.StoreImageAsync(
            dto.CohortId,
            userId,
            dto.File,
            cancellationToken);

        if (descriptor.MediaType != ChatMediaType.Image)
        {
            throw new CohortValidationException("Unsupported media type.");
        }

        return new UploadMediaResultDto
        {
            CohortId = dto.CohortId,
            UserId = userId,
            MediaKey = descriptor.Key,
            MediaUrl = descriptor.Url,
            ContentType = descriptor.ContentType,
            SizeBytes = descriptor.SizeBytes
        };
    }
}