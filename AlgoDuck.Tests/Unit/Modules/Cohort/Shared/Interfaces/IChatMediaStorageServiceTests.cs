using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Services;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Shared.S3;
using Amazon.S3;
using Amazon.S3.Model;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Shared.Interfaces;

public sealed class IChatMediaStorageServiceTests
{
    [Fact]
    public void StoreImageAsync_IsImplementedByChatMediaStorageService()
    {
        typeof(ChatMediaStorageService)
            .GetInterfaces()
            .Should()
            .Contain(i => i == typeof(IChatMediaStorageService));
    }

    [Fact]
    public async Task StoreImageAsync_WhenCalledWithValidParameters_CompletesSuccessfully()
    {
        var s3ClientMock = new Mock<IAmazonS3>();
        s3ClientMock
            .Setup(c => c.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PutObjectResponse
            {
                HttpStatusCode = System.Net.HttpStatusCode.OK
            });

        var mediaSettings = Options.Create(new ChatMediaSettings
        {
            RootPrefix = "chat",
            MaxFileSizeBytes = 10_000_000,
            AllowedContentTypes = new[] { "image/png" }
        });

        var s3Settings = Options.Create(new S3Settings
        {
            ContentBucketSettings = new S3BucketSettings
            {
                BucketName = "test-bucket",
                Region = "us-east-1",
                Type = S3BucketType.Content
            },
            DataBucketSettings = new S3BucketSettings
            {
                BucketName = "data-bucket",
                Region = "us-east-1",
                Type = S3BucketType.Data
            }
        });

        var formFileMock = new Mock<IFormFile>();
        var fileContent = new byte[] { 1, 2, 3 };
        var fileLength = fileContent.Length;

        formFileMock
            .Setup(f => f.OpenReadStream())
            .Returns(() => new MemoryStream(fileContent));

        formFileMock.Setup(f => f.Length).Returns(fileLength);
        formFileMock.Setup(f => f.FileName).Returns("image.png");
        formFileMock.Setup(f => f.ContentType).Returns("image/png");

        IChatMediaStorageService service = new ChatMediaStorageService(
            s3ClientMock.Object,
            mediaSettings,
            s3Settings);

        var cohortId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var result = await service.StoreImageAsync(cohortId, userId, formFileMock.Object, CancellationToken.None);

        result.Url.Should().NotBeNullOrWhiteSpace();
        result.ContentType.Should().Be("image/png");
        result.MediaType.Should().Be(ChatMediaType.Image);
        result.SizeBytes.Should().Be(fileLength);
        result.Key.Should().NotBeNullOrWhiteSpace();
    }
}