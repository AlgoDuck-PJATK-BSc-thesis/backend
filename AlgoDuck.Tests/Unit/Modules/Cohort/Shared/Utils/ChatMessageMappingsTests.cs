using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Shared.Utils;

public sealed class ChatMessageMappingsTests
{
    [Fact]
    public void ToSendMessageResultDto_MapsAllFields()
    {
        var messageId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var message = new Message
        {
            MessageId = messageId,
            CohortId = cohortId,
            UserId = userId,
            Message1 = "Hello world",
            CreatedAt = createdAt
        };

        var profile = new UserProfileDto
        {
            UserId = userId,
            Username = "alice",
            S3AvatarUrl = "https://example.com/avatar.png"
        };

        var mediaType = ChatMediaType.Image;
        var mediaUrl = "https://example.com/image.png";

        var result = ChatMessageMappings.ToSendMessageResultDto(
            message,
            profile,
            mediaType,
            mediaUrl);

        Assert.Equal(messageId, result.MessageId);
        Assert.Equal(cohortId, result.CohortId);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("alice", result.UserName);
        Assert.Equal("https://example.com/avatar.png", result.UserAvatarUrl);
        Assert.Equal("Hello world", result.Content);
        Assert.Equal(mediaType, result.MediaType);
        Assert.Equal(mediaUrl, result.MediaUrl);
        Assert.Equal(createdAt, result.CreatedAt);
    }

    [Fact]
    public void ToGetCohortMessagesItemDto_MapsFieldsAndIsMineFlag()
    {
        var messageId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var message = new Message
        {
            MessageId = messageId,
            CohortId = cohortId,
            UserId = authorId,
            Message1 = "Hello cohort",
            CreatedAt = createdAt
        };

        var profile = new UserProfileDto
        {
            UserId = authorId,
            Username = "bob",
            S3AvatarUrl = "https://example.com/avatar-bob.png"
        };

        var mediaType = ChatMediaType.Text;
        string? mediaUrl = null;

        var mine = ChatMessageMappings.ToGetCohortMessagesItemDto(
            message,
            profile,
            authorId,
            mediaType,
            mediaUrl);

        var notMine = ChatMessageMappings.ToGetCohortMessagesItemDto(
            message,
            profile,
            otherUserId,
            mediaType,
            mediaUrl);

        Assert.Equal(messageId, mine.MessageId);
        Assert.Equal(cohortId, mine.CohortId);
        Assert.Equal(authorId, mine.UserId);
        Assert.Equal("bob", mine.UserName);
        Assert.Equal("https://example.com/avatar-bob.png", mine.UserAvatarUrl);
        Assert.Equal("Hello cohort", mine.Content);
        Assert.Equal(mediaType, mine.MediaType);
        Assert.Null(mine.MediaUrl);
        Assert.Equal(createdAt, mine.CreatedAt);
        Assert.True(mine.IsMine);

        Assert.False(notMine.IsMine);
    }
}