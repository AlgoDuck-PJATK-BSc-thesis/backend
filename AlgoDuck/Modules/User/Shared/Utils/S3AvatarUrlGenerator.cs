using AlgoDuck.Modules.User.Shared.Interfaces;
using AlgoDuck.Shared.S3;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.User.Shared.Utils;

public sealed class S3AvatarUrlGenerator : IS3AvatarUrlGenerator
{
    private readonly S3Settings _settings;

    public S3AvatarUrlGenerator(IOptions<S3Settings> options)
    {
        _settings = options.Value;
    }

    public string GetAvatarUrl(string avatarKey)
    {
        if (string.IsNullOrWhiteSpace(avatarKey))
            return string.Empty;

        var bucket = _settings.ContentBucketSettings;

        return $"https://{bucket.BucketName}.s3.{bucket.Region}.amazonaws.com/{avatarKey}";
    }
}