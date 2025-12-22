using System.Text;
using AlgoDuck.Modules.Cohort.Commands.Chat.UploadMedia;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Tests.Modules.Cohort.Commands.Chat.UploadMedia;

public class UploadMediaValidatorTests
{
    UploadMediaValidator CreateValidator(long maxSizeBytes, params string[] allowedContentTypes)
    {
        var settings = new ChatMediaSettings
        {
            MaxFileSizeBytes = maxSizeBytes,
            AllowedContentTypes = allowedContentTypes
        };

        var options = Options.Create(settings);
        return new UploadMediaValidator(options);
    }

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
    public void Validate_WhenCohortIdIsEmpty_ThenHasValidationError()
    {
        var validator = CreateValidator(10_000_000, "image/png");
        var dto = new UploadMediaDto
        {
            CohortId = Guid.Empty,
            File = CreateFormFile(Encoding.UTF8.GetBytes("data"), "file.png", "image/png")
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.CohortId);
    }

    [Fact]
    public void Validate_WhenFileIsNull_ThenHasValidationError()
    {
        var validator = CreateValidator(10_000_000, "image/png");
        var dto = new UploadMediaDto
        {
            CohortId = Guid.NewGuid(),
            File = null!
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public void Validate_WhenFileIsEmpty_ThenHasValidationError()
    {
        var validator = CreateValidator(10_000_000, "image/png");
        var emptyBytes = Array.Empty<byte>();
        var dto = new UploadMediaDto
        {
            CohortId = Guid.NewGuid(),
            File = CreateFormFile(emptyBytes, "file.png", "image/png")
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public void Validate_WhenFileTooLarge_ThenHasValidationError()
    {
        var validator = CreateValidator(10, "image/png");
        var bytes = Encoding.UTF8.GetBytes("this is more than ten bytes");
        var dto = new UploadMediaDto
        {
            CohortId = Guid.NewGuid(),
            File = CreateFormFile(bytes, "file.png", "image/png")
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public void Validate_WhenContentTypeNotAllowed_ThenHasValidationError()
    {
        var validator = CreateValidator(10_000_000, "image/png");
        var bytes = Encoding.UTF8.GetBytes("data");
        var dto = new UploadMediaDto
        {
            CohortId = Guid.NewGuid(),
            File = CreateFormFile(bytes, "file.jpg", "image/jpeg")
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public void Validate_WhenAllGood_ThenHasNoValidationErrors()
    {
        var validator = CreateValidator(10_000_000, "image/png", "image/jpeg");
        var bytes = Encoding.UTF8.GetBytes("data");
        var dto = new UploadMediaDto
        {
            CohortId = Guid.NewGuid(),
            File = CreateFormFile(bytes, "file.png", "image/png")
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}