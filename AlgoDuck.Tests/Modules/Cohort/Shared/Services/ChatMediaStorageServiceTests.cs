using System.Net;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
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
    private static ChatMediaStorageService CreateService(out Mock<IAmazonS3> s3Mock, out ChatMediaSettings mediaSettings)
    {
        s3Mock = new Mock<IAmazonS3>();

        mediaSettings = new ChatMediaSettings
        {
            RootPrefix = "chat-media",
            MaxFileSizeBytes = 1024 * 1024,
            AllowedContentTypes = new[] { "image/png", "image/jpeg" }
        };

        var s3Settings = new S3Settings
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

        return new ChatMediaStorageService(
            s3Mock.Object,
            Options.Create(mediaSettings),
            Options.Create(s3Settings));
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
    public async Task StoreImageAsync_WhenFileIsNull_ThrowsCohortValidationException()
    {
        var service = CreateService(out var s3Mock, out _);

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            service.StoreImageAsync(Guid.NewGuid(), Guid.NewGuid(), null!, CancellationToken.None));

        ex.Message.Should().Be("File is required.");
        ex.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        s3Mock.Verify(
            x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task StoreImageAsync_WhenFileIsEmpty_ThrowsCohortValidationException()
    {
        var service = CreateService(out var s3Mock, out _);
        var file = CreateFormFile("image.png", "image/png", Array.Empty<byte>());

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            service.StoreImageAsync(Guid.NewGuid(), Guid.NewGuid(), file, CancellationToken.None));

        ex.Message.Should().Be("File is empty.");
        ex.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        s3Mock.Verify(
            x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task StoreImageAsync_WhenFileTooLarge_ThrowsCohortValidationException()
    {
        var service = CreateService(out var s3Mock, out var mediaSettings);

        var oversized = new byte[mediaSettings.MaxFileSizeBytes + 1];
        var file = CreateFormFile("image.png", "image/png", oversized);

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            service.StoreImageAsync(Guid.NewGuid(), Guid.NewGuid(), file, CancellationToken.None));

        ex.Message.Should().Be("File is too large.");
        ex.StatusCode.Should().Be(StatusCodes.Status413PayloadTooLarge);

        s3Mock.Verify(
            x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task StoreImageAsync_WhenContentTypeNotAllowed_ThrowsCohortValidationException()
    {
        var service = CreateService(out var s3Mock, out _);

        var bytes = new byte[16];
        var file = CreateFormFile("doc.txt", "text/plain", bytes);

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            service.StoreImageAsync(Guid.NewGuid(), Guid.NewGuid(), file, CancellationToken.None));

        ex.Message.Should().Be("Unsupported media type.");
        ex.StatusCode.Should().Be(StatusCodes.Status415UnsupportedMediaType);

        s3Mock.Verify(
            x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task StoreImageAsync_WhenS3UploadFails_ThrowsCohortValidationException()
    {
        var service = CreateService(out var s3Mock, out _);

        s3Mock
            .Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse { HttpStatusCode = HttpStatusCode.BadRequest });

        var bytes = new byte[64];
        var file = CreateFormFile("image.png", "image/png", bytes);

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            service.StoreImageAsync(Guid.NewGuid(), Guid.NewGuid(), file, CancellationToken.None));

        ex.Message.Should().Be("Failed to upload chat media to storage.");
        ex.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        s3Mock.Verify(
            x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StoreImageAsync_WhenUploadSucceeds_ReturnsDescriptorWithExpectedValues()
    {
        var service = CreateService(out var s3Mock, out _);

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
        result.Url.Should().Be($"/api/cohorts/{cohortId}/chat/media?key={Uri.EscapeDataString(result.Key)}");
    }
}
