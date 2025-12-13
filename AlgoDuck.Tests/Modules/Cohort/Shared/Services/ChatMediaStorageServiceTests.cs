using System.Net;
using AlgoDuck.Modules.Cohort.Shared.Services;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Shared.S3;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using FluentAssertions;

namespace AlgoDuck.Tests.Modules.Cohort.Shared.Services;

public sealed class ChatMediaStorageServiceTests
{
    private static ChatMediaStorageService CreateService(
        out Mock<IAmazonS3> s3Mock,
        out ChatMediaSettings mediaSettings,
        out S3Settings s3Settings)
    {
        s3Mock = new Mock<IAmazonS3>();

        mediaSettings = new ChatMediaSettings
        {
            RootPrefix = "chat-media",
            MaxFileSizeBytes = 1024 * 1024,
            AllowedContentTypes = new[] { "image/png", "image/jpeg" }
        };

        s3Settings = new S3Settings
        {
            ContentBucketSettings = new S3BucketSettings
            {
                BucketName = "content-bucket",
                Region = "us-east-1",
                Type = S3BucketType.Content
            },
            DataBucketSettings = new S3BucketSettings
            {
                BucketName = "data-bucket",
                Region = "us-east-1",
                Type = S3BucketType.Data
            }
        };

        var mediaOptions = Options.Create(mediaSettings);
        var s3Options = Options.Create(s3Settings);

        return new ChatMediaStorageService(
            s3Mock.Object,
            mediaOptions,
            s3Options);
    }

    private static IFormFile CreateFormFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    [Fact]
    public async Task StoreImageAsync_WhenFileIsNull_ThrowsArgumentNullException()
    {
        var service = CreateService(out var s3Mock, out _, out _);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.StoreImageAsync(Guid.NewGuid(), Guid.NewGuid(), null!, CancellationToken.None));

        s3Mock.Verify(
            x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task StoreImageAsync_WhenFileIsEmpty_ThrowsInvalidOperationException()
    {
        var service = CreateService(out var s3Mock, out _, out _);
        var file = CreateFormFile("image.png", "image/png", Array.Empty<byte>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StoreImageAsync(Guid.NewGuid(), Guid.NewGuid(), file, CancellationToken.None));

        s3Mock.Verify(
            x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task StoreImageAsync_WhenFileTooLarge_ThrowsInvalidOperationException()
    {
        var service = CreateService(out var s3Mock, out var mediaSettings, out _);

        var oversized = new byte[mediaSettings.MaxFileSizeBytes + 1];
        var file = CreateFormFile("image.png", "image/png", oversized);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StoreImageAsync(Guid.NewGuid(), Guid.NewGuid(), file, CancellationToken.None));

        s3Mock.Verify(
            x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task StoreImageAsync_WhenContentTypeNotAllowed_ThrowsInvalidOperationException()
    {
        var service = CreateService(out var s3Mock, out _, out _);

        var bytes = new byte[16];
        var file = CreateFormFile("doc.txt", "text/plain", bytes);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StoreImageAsync(Guid.NewGuid(), Guid.NewGuid(), file, CancellationToken.None));

        s3Mock.Verify(
            x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task StoreImageAsync_WhenS3UploadFails_ThrowsInvalidOperationException()
    {
        var service = CreateService(out var s3Mock, out _, out _);

        s3Mock
            .Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.BadRequest });

        var bytes = new byte[64];
        var file = CreateFormFile("image.png", "image/png", bytes);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.StoreImageAsync(Guid.NewGuid(), Guid.NewGuid(), file, CancellationToken.None));

        s3Mock.Verify(
            x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StoreImageAsync_WhenUploadSucceeds_ReturnsDescriptorWithExpectedValues()
    {
        var service = CreateService(out var s3Mock, out _, out var s3Settings);

        s3Mock
            .Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.OK });

        var bytes = new byte[128];
        var file = CreateFormFile("photo.jpg", "image/jpeg", bytes);

        var cohortId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var result = await service.StoreImageAsync(cohortId, userId, file, CancellationToken.None);

        result.Should().NotBeNull();
        result.ContentType.Should().Be("image/jpeg");
        result.SizeBytes.Should().Be(bytes.Length);
        result.MediaType.Should().Be(ChatMediaType.Image);
        result.Key.Should().NotBeNullOrWhiteSpace();
        result.Url.Should().StartWith($"https://{s3Settings.ContentBucketSettings.BucketName}.s3.{s3Settings.ContentBucketSettings.Region}.amazonaws.com/");
    }
}