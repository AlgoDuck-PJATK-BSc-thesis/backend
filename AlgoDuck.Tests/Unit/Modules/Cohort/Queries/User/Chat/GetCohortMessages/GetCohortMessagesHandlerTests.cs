using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Queries.User.Chat.GetCohortMessages;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Queries.User.Chat.GetCohortMessages;

public sealed class GetCohortMessagesHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRequestInvalid_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<GetCohortMessagesRequestDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var chatRepositoryMock = new Mock<IChatMessageRepository>();
        var profileServiceMock = new Mock<IProfileService>();

        var dto = new GetCohortMessagesRequestDto
        {
            CohortId = Guid.Empty
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure(nameof(GetCohortMessagesRequestDto.CohortId), "Invalid")
            }));

        var handler = new GetCohortMessagesHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            chatRepositoryMock.Object,
            profileServiceMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotInCohort_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<GetCohortMessagesRequestDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var chatRepositoryMock = new Mock<IChatMessageRepository>();
        var profileServiceMock = new Mock<IProfileService>();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new GetCohortMessagesRequestDto
        {
            CohortId = cohortId,
            PageSize = 10
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(r => r.UserBelongsToCohortAsync(userId, cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new GetCohortMessagesHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            chatRepositoryMock.Object,
            profileServiceMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenNoMessages_ThenReturnsEmptyItems()
    {
        var validatorMock = new Mock<IValidator<GetCohortMessagesRequestDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var chatRepositoryMock = new Mock<IChatMessageRepository>();
        var profileServiceMock = new Mock<IProfileService>();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new GetCohortMessagesRequestDto
        {
            CohortId = cohortId,
            PageSize = 10
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(r => r.UserBelongsToCohortAsync(userId, cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        chatRepositoryMock
            .Setup(r => r.GetPagedForCohortAsync(
                cohortId,
                dto.BeforeCreatedAt,
                dto.PageSize!.Value + 1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Message>());

        var handler = new GetCohortMessagesHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            chatRepositoryMock.Object,
            profileServiceMock.Object);

        var result = await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.False(result.HasMore);
        Assert.Null(result.NextCursor);
    }

    [Fact]
    public async Task HandleAsync_WhenMessagesExist_ThenMapsProfilesAndPaging()
    {
        var validatorMock = new Mock<IValidator<GetCohortMessagesRequestDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var chatRepositoryMock = new Mock<IChatMessageRepository>();
        var profileServiceMock = new Mock<IProfileService>();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var dto = new GetCohortMessagesRequestDto
        {
            CohortId = cohortId,
            PageSize = 2
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(r => r.UserBelongsToCohortAsync(userId, cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var now = DateTime.UtcNow;

        var message1 = new Message
        {
            MessageId = Guid.NewGuid(),
            CohortId = cohortId,
            UserId = userId,
            Message1 = "hi",
            CreatedAt = now.AddMinutes(-1),
            MediaType = (int)ChatMediaType.Text
        };

        var message2 = new Message
        {
            MessageId = Guid.NewGuid(),
            CohortId = cohortId,
            UserId = Guid.NewGuid(),
            Message1 = "hello",
            CreatedAt = now.AddMinutes(-2),
            MediaType = (int)ChatMediaType.Text
        };

        var message3 = new Message
        {
            MessageId = Guid.NewGuid(),
            CohortId = cohortId,
            UserId = Guid.NewGuid(),
            Message1 = "extra",
            CreatedAt = now.AddMinutes(-3),
            MediaType = (int)ChatMediaType.Text
        };

        chatRepositoryMock
            .Setup(r => r.GetPagedForCohortAsync(
                cohortId,
                dto.BeforeCreatedAt,
                dto.PageSize!.Value + 1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Message> { message1, message2, message3 });

        profileServiceMock
            .Setup(p => p.GetProfileAsync(message1.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileDto
            {
                UserId = message1.UserId,
                Username = "me",
                S3AvatarUrl = "url1"
            });

        profileServiceMock
            .Setup(p => p.GetProfileAsync(message2.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileDto
            {
                UserId = message2.UserId,
                Username = "other",
                S3AvatarUrl = "url2"
            });

        var handler = new GetCohortMessagesHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            chatRepositoryMock.Object,
            profileServiceMock.Object);

        var result = await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.True(result.HasMore);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, x => x.MessageId == message1.MessageId);
        Assert.Contains(result.Items, x => x.MessageId == message2.MessageId);
        Assert.NotNull(result.NextCursor);
    }

    [Fact]
    public async Task HandleAsync_WhenImageMessageExists_ThenReturnsMediaUrl()
    {
        var validatorMock = new Mock<IValidator<GetCohortMessagesRequestDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var chatRepositoryMock = new Mock<IChatMessageRepository>();
        var profileServiceMock = new Mock<IProfileService>();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();
        var key = "chat/cohorts/test/image.png";

        var dto = new GetCohortMessagesRequestDto
        {
            CohortId = cohortId,
            PageSize = 10
        };

        validatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(r => r.UserBelongsToCohortAsync(userId, cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var message = new Message
        {
            MessageId = Guid.NewGuid(),
            CohortId = cohortId,
            UserId = Guid.NewGuid(),
            Message1 = "",
            CreatedAt = DateTime.UtcNow,
            MediaType = (int)ChatMediaType.Image,
            MediaKey = key,
            MediaContentType = "image/png"
        };

        chatRepositoryMock
            .Setup(r => r.GetPagedForCohortAsync(
                cohortId,
                dto.BeforeCreatedAt,
                dto.PageSize!.Value + 1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Message> { message });

        profileServiceMock
            .Setup(p => p.GetProfileAsync(message.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileDto
            {
                UserId = message.UserId,
                Username = "u",
                S3AvatarUrl = "a"
            });

        var handler = new GetCohortMessagesHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            chatRepositoryMock.Object,
            profileServiceMock.Object);

        var result = await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(ChatMediaType.Image, result.Items[0].MediaType);
        Assert.Equal($"/api/cohorts/{cohortId}/chat/media?key={Uri.EscapeDataString(key)}", result.Items[0].MediaUrl);
    }
}
