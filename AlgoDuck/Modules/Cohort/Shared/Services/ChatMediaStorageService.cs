using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Shared.S3;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Cohort.Shared.Services;

public sealed class ChatMediaStorageService : IChatMediaStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ChatMediaSettings _mediaSettings;
    private readonly S3Settings _s3Settings;

    public ChatMediaStorageService(
        IAmazonS3 s3Client,
        IOptions<ChatMediaSettings> mediaOptions,
        IOptions<S3Settings> s3Options)
    {
        _s3Client = s3Client;
        _mediaSettings = mediaOptions.Value;
        _s3Settings = s3Options.Value;
    }

    public async Task<ChatMediaDescriptor> StoreImageAsync(
        Guid cohortId,
        Guid userId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null)
        {
            throw new CohortValidationException("File is required.", StatusCodes.Status400BadRequest);
        }

        if (file.Length == 0)
        {
            throw new CohortValidationException("File is empty.", StatusCodes.Status400BadRequest);
        }

        if (file.Length > _mediaSettings.MaxFileSizeBytes)
        {
            throw new CohortValidationException("File is too large.", StatusCodes.Status413PayloadTooLarge);
        }

        if (!_mediaSettings.AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new CohortValidationException("Unsupported media type.", StatusCodes.Status415UnsupportedMediaType);
        }

        var extension = Path.GetExtension(file.FileName);
        var key = BuildObjectKey(cohortId, userId, extension);

        await using var stream = file.OpenReadStream();

        var bucket = _s3Settings.ContentBucketSettings;

        var request = new PutObjectRequest
        {
            BucketName = bucket.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = file.ContentType
        };

        var response = await _s3Client.PutObjectAsync(request, cancellationToken);

        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new CohortValidationException("Failed to upload chat media to storage.", StatusCodes.Status500InternalServerError);
        }

        var url = $"https://{bucket.BucketName}.s3.{bucket.Region}.amazonaws.com/{key}";

        return new ChatMediaDescriptor
        {
            Key = key,
            Url = url,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            MediaType = ChatMediaType.Image
        };
    }

    private string BuildObjectKey(Guid cohortId, Guid userId, string extension)
    {
        var sanitizedExtension = string.IsNullOrWhiteSpace(extension) ? string.Empty : extension;
        var fileName = $"{Guid.NewGuid()}{sanitizedExtension}";
        return $"{_mediaSettings.RootPrefix}/cohorts/{cohortId}/users/{userId}/{fileName}";
    }
}
