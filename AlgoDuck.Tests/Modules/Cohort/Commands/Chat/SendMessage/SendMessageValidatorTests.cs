using AlgoDuck.Modules.Cohort.Commands.Chat.SendMessage;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.Cohort.Commands.Chat.SendMessage;

public class SendMessageValidatorTests
{
    SendMessageValidator CreateValidator()
    {
        return new SendMessageValidator();
    }

    [Fact]
    public void Validate_WhenCohortIdIsEmpty_ThenHasValidationError()
    {
        var validator = CreateValidator();
        var dto = new SendMessageDto
        {
            CohortId = Guid.Empty,
            Content = "hello",
            MediaType = ChatMediaType.Text
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.CohortId);
    }

    [Fact]
    public void Validate_WhenTextMessageContentIsEmpty_ThenHasValidationError()
    {
        var validator = CreateValidator();
        var dto = new SendMessageDto
        {
            CohortId = Guid.NewGuid(),
            Content = "",
            MediaType = ChatMediaType.Text
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Validate_WhenTextMessageTooLong_ThenHasValidationError()
    {
        var validator = CreateValidator();
        var longContent = new string('a', ChatConstants.MaxMessageLength + 1);

        var dto = new SendMessageDto
        {
            CohortId = Guid.NewGuid(),
            Content = longContent,
            MediaType = ChatMediaType.Text
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void Validate_WhenValidTextMessage_ThenHasNoValidationErrors()
    {
        var validator = CreateValidator();
        var content = new string('a', ChatConstants.MaxMessageLength);

        var dto = new SendMessageDto
        {
            CohortId = Guid.NewGuid(),
            Content = content,
            MediaType = ChatMediaType.Text
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenImageMissingMediaKey_ThenHasValidationError()
    {
        var validator = CreateValidator();

        var dto = new SendMessageDto
        {
            CohortId = Guid.NewGuid(),
            MediaType = ChatMediaType.Image,
            MediaKey = null,
            MediaContentType = "image/png"
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MediaKey);
    }

    [Fact]
    public void Validate_WhenImageMissingContentType_ThenHasValidationError()
    {
        var validator = CreateValidator();

        var dto = new SendMessageDto
        {
            CohortId = Guid.NewGuid(),
            MediaType = ChatMediaType.Image,
            MediaKey = "cohorts/123/media/file.png",
            MediaContentType = null
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MediaContentType);
    }

    [Fact]
    public void Validate_WhenValidImageMessage_ThenHasNoValidationErrors()
    {
        var validator = CreateValidator();

        var dto = new SendMessageDto
        {
            CohortId = Guid.NewGuid(),
            MediaType = ChatMediaType.Image,
            MediaKey = "cohorts/123/media/file.png",
            MediaContentType = "image/png"
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}