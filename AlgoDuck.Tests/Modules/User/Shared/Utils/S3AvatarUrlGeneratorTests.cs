using AlgoDuck.Modules.User.Shared.Utils;
using AlgoDuck.Shared.S3;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Tests.Modules.User.Shared.Utils;

public sealed class S3AvatarUrlGeneratorTests
{
    [Fact]
    public void GetAvatarUrl_WhenAvatarKeyIsNull_ReturnsEmptyString()
    {
        var s3Settings = new S3Settings
        {
            ContentBucketSettings = new S3BucketSettings
            {
                BucketName = "content-bucket",
                Region = "eu-central-1",
                Type = S3BucketType.Content
            },
            DataBucketSettings = new S3BucketSettings
            {
                BucketName = "data-bucket",
                Region = "eu-central-1",
                Type = S3BucketType.Data
            }
        };

        var options = Options.Create(s3Settings);
        var generator = new S3AvatarUrlGenerator(options);

        var result = generator.GetAvatarUrl(null!);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAvatarUrl_WhenAvatarKeyIsWhitespace_ReturnsEmptyString()
    {
        var s3Settings = new S3Settings
        {
            ContentBucketSettings = new S3BucketSettings
            {
                BucketName = "content-bucket",
                Region = "eu-central-1",
                Type = S3BucketType.Content
            },
            DataBucketSettings = new S3BucketSettings
            {
                BucketName = "data-bucket",
                Region = "eu-central-1",
                Type = S3BucketType.Data
            }
        };

        var options = Options.Create(s3Settings);
        var generator = new S3AvatarUrlGenerator(options);

        var result = generator.GetAvatarUrl("   ");

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAvatarUrl_WhenAvatarKeyIsValid_ReturnsCorrectUrl()
    {
        var s3Settings = new S3Settings
        {
            ContentBucketSettings = new S3BucketSettings
            {
                BucketName = "content-bucket",
                Region = "eu-central-1",
                Type = S3BucketType.Content
            },
            DataBucketSettings = new S3BucketSettings
            {
                BucketName = "data-bucket",
                Region = "eu-central-1",
                Type = S3BucketType.Data
            }
        };

        var options = Options.Create(s3Settings);
        var generator = new S3AvatarUrlGenerator(options);

        const string avatarKey = "avatars/user123.png";

        var result = generator.GetAvatarUrl(avatarKey);

        result.Should().Be("https://content-bucket.s3.eu-central-1.amazonaws.com/avatars/user123.png");
    }
}