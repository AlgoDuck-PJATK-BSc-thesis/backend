using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Commands.Chat.SendMessage;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Moq;

namespace AlgoDuck.Tests.Modules.Cohort.Commands.Chat.SendMessage;

public class SendMessageHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenDtoIsInvalid_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<SendMessageDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var chatMessageRepositoryMock = new Mock<IChatMessageRepository>();
        var chatModerationServiceMock = new Mock<IChatModerationService>();
        var profileServiceMock = new Mock<IProfileService>();

        var userId = Guid.NewGuid();
        var dto = new SendMessageDto
        {
            CohortId = Guid.Empty,
            Content = ""
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure(nameof(SendMessageDto.CohortId), "Required")
            }));

        var handler = new SendMessageHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            chatMessageRepositoryMock.Object,
            chatModerationServiceMock.Object,
            profileServiceMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotInCohort_ThenThrowsCohortValidationException()
    {
        var validatorMock = new Mock<IValidator<SendMessageDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var chatMessageRepositoryMock = new Mock<IChatMessageRepository>();
        var chatModerationServiceMock = new Mock<IChatModerationService>();
        var profileServiceMock = new Mock<IProfileService>();

        var userId = Guid.NewGuid();
        var dto = new SendMessageDto
        {
            CohortId = Guid.NewGuid(),
            Content = "hello world",
            MediaType = ChatMediaType.Text
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(x => x.UserBelongsToCohortAsync(userId, dto.CohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new SendMessageHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            chatMessageRepositoryMock.Object,
            chatModerationServiceMock.Object,
            profileServiceMock.Object);

        await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        cohortRepositoryMock.Verify(x => x.UserBelongsToCohortAsync(userId, dto.CohortId, It.IsAny<CancellationToken>()), Times.Once);
        chatModerationServiceMock.Verify(x => x.CheckMessageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        chatMessageRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTextMessageValidAndAllowed_ThenSavesMessageAndReturnsResult()
    {
        var validatorMock = new Mock<IValidator<SendMessageDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var chatMessageRepositoryMock = new Mock<IChatMessageRepository>();
        var chatModerationServiceMock = new Mock<IChatModerationService>();
        var profileServiceMock = new Mock<IProfileService>();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();
        var dto = new SendMessageDto
        {
            CohortId = cohortId,
            Content = "hello world",
            MediaType = ChatMediaType.Text
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(x => x.UserBelongsToCohortAsync(userId, cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        chatModerationServiceMock
            .Setup(x => x.CheckMessageAsync(userId, cohortId, dto.Content, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ChatModerationResult.Allowed());

        var savedMessage = new Message
        {
            MessageId = Guid.NewGuid(),
            CohortId = cohortId,
            UserId = userId,
            Message1 = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        chatMessageRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedMessage);

        var profile = new UserProfileDto
        {
            UserId = userId,
            Username = "alice",
            S3AvatarUrl = "https://cdn.algoduck.test/avatar/alice.png"
        };

        profileServiceMock
            .Setup(x => x.GetProfileAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var handler = new SendMessageHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            chatMessageRepositoryMock.Object,
            chatModerationServiceMock.Object,
            profileServiceMock.Object);

        var result = await handler.HandleAsync(userId, dto, CancellationToken.None);

        Assert.Equal(savedMessage.MessageId, result.MessageId);
        Assert.Equal(cohortId, result.CohortId);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(profile.Username, result.UserName);
        Assert.Equal(profile.S3AvatarUrl, result.UserAvatarUrl);
        Assert.Equal(savedMessage.Message1, result.Content);
        Assert.Equal(ChatMediaType.Text, result.MediaType);
        Assert.Equal(savedMessage.CreatedAt, result.CreatedAt);

        validatorMock.Verify(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
        cohortRepositoryMock.Verify(x => x.UserBelongsToCohortAsync(userId, cohortId, It.IsAny<CancellationToken>()), Times.Once);
        chatModerationServiceMock.Verify(x => x.CheckMessageAsync(userId, cohortId, dto.Content, It.IsAny<CancellationToken>()), Times.Once);
        chatMessageRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Once);
        profileServiceMock.Verify(x => x.GetProfileAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenModerationBlocksMessage_ThenThrowsChatValidationException()
    {
        var validatorMock = new Mock<IValidator<SendMessageDto>>();
        var cohortRepositoryMock = new Mock<ICohortRepository>();
        var chatMessageRepositoryMock = new Mock<IChatMessageRepository>();
        var chatModerationServiceMock = new Mock<IChatModerationService>();
        var profileServiceMock = new Mock<IProfileService>();

        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();
        var dto = new SendMessageDto
        {
            CohortId = cohortId,
            Content = "bad content",
            MediaType = ChatMediaType.Text
        };

        validatorMock
            .Setup(x => x.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        cohortRepositoryMock
            .Setup(x => x.UserBelongsToCohortAsync(userId, cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var moderationResult = ChatModerationResult.Blocked("not allowed", "toxicity");

        chatModerationServiceMock
            .Setup(x => x.CheckMessageAsync(userId, cohortId, dto.Content, It.IsAny<CancellationToken>()))
            .ReturnsAsync(moderationResult);

        var handler = new SendMessageHandler(
            validatorMock.Object,
            cohortRepositoryMock.Object,
            chatMessageRepositoryMock.Object,
            chatModerationServiceMock.Object,
            profileServiceMock.Object);

        await Assert.ThrowsAsync<ChatValidationException>(() =>
            handler.HandleAsync(userId, dto, CancellationToken.None));

        chatMessageRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Never);
        profileServiceMock.Verify(x => x.GetProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}