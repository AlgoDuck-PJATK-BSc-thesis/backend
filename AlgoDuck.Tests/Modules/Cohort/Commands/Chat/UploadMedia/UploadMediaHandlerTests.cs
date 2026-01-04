using System.Text;
using AlgoDuck.Modules.Cohort.Commands.User.Chat.UploadMedia;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AlgoDuck.Tests.Modules.Cohort.Commands.Chat.UploadMedia;

public class UploadMediaHandlerTests
{
    IFormFile CreateFormFile(byte[] content, string fileName, string contentType)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.LongLength, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<UploadMediaDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var mediaStorageMock = new Mock<IChatMediaStorageService>();

        var dto = new UploadMediaDto
        {
            CohortId = Guid.Empty,
            File = CreateFormFile(Encoding.UTF8.GetBytes("data"), "file.png", "image/png")
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure(nameof(UploadMediaDto.CohortId), "Required")
            }));

        var handler = new UploadMediaHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            mediaStorageMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), dto, CancellationToken.None));

        cohortRepositoryMock.Verify(x => x.UserBelongsToCohortAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        mediaStorageMock.Verify(x => x.StoreImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotInCohort_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<UploadMediaDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var mediaStorageMock = new Mock<IChatMediaStorageService>();

        var dto = new UploadMediaDto
        {
            CohortId = Guid.NewGuid(),
            File = CreateFormFile(Encoding.UTF8.GetBytes("data"), "file.png", "image/png")
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(x => x.UserBelongsToCohortAsync(It.IsAny<Guid>(), dto.CohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new UploadMediaHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            mediaStorageMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), dto, CancellationToken.None));

        cohortRepositoryMock.Verify(x => x.UserBelongsToCohortAsync(It.IsAny<Guid>(), dto.CohortId, It.IsAny<CancellationToken>()), Times.Once);
        mediaStorageMock.Verify(x => x.StoreImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenMediaTypeIsNotImage_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<UploadMediaDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var mediaStorageMock = new Mock<IChatMediaStorageService>();
        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new UploadMediaDto
        {
            CohortId = cohortId,
            File = CreateFormFile(Encoding.UTF8.GetBytes("data"), "file.png", "image/png")
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(x => x.UserBelongsToCohortAsync(userId, cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var descriptor = new ChatMediaDescriptor
        {
            Key = "media/123",
            Url = "https://cdn.example.com/media/123",
            ContentType = "application/octet-stream",
            SizeBytes = dto.File.Length,
            MediaType = ChatMediaType.Text
        };

        mediaStorageMock
            .Setup(x => x.StoreImageAsync(cohortId, userId, It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(descriptor);

        var handler = new UploadMediaHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            mediaStorageMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        mediaStorageMock.Verify(x => x.StoreImageAsync(cohortId, userId, It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenAllGood_ThenReturnsResultDto()
    {
        var validatorMock = new Mock<IValidator<UploadMediaDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var mediaStorageMock = new Mock<IChatMediaStorageService>();
        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new UploadMediaDto
        {
            CohortId = cohortId,
            File = CreateFormFile(Encoding.UTF8.GetBytes("data"), "file.png", "image/png")
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(x => x.UserBelongsToCohortAsync(userId, cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var descriptor = new ChatMediaDescriptor
        {
            Key = "media/123",
            Url = "https://cdn.example.com/media/123",
            ContentType = "image/png",
            SizeBytes = dto.File.Length,
            MediaType = ChatMediaType.Image
        };

        mediaStorageMock
            .Setup(x => x.StoreImageAsync(cohortId, userId, It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(descriptor);

        var handler = new UploadMediaHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            mediaStorageMock.Object);

        var result = await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.Equal(cohortId, result.CohortId);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(descriptor.Key, result.MediaKey);
        Assert.Equal(descriptor.Url, result.MediaUrl);
        Assert.Equal(descriptor.ContentType, result.ContentType);
        Assert.Equal(descriptor.SizeBytes, result.SizeBytes);

        cohortRepositoryMock.Verify(x => x.UserBelongsToCohortAsync(userId, cohortId, It.IsAny<CancellationToken>()), Times.Once);
        mediaStorageMock.Verify(x => x.StoreImageAsync(cohortId, userId, It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}