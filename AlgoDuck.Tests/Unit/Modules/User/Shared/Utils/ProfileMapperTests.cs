using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Utils;
using FluentAssertions;

namespace AlgoDuck.Tests.Unit.Modules.User.Shared.Utils;

public sealed class ProfileMapperTests
{
    [Fact]
    public void ToUserProfileDto_WithCompleteUser_PopulatesBasicFields()
    {
        var userId = Guid.NewGuid();
        var cohortId = Guid.NewGuid();

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "test-user",
            Email = "user@example.com",
            Coins = 123,
            Experience = 456,
            AmountSolved = 7,
            CohortId = cohortId
        };

        var result = ProfileMapper.ToUserProfileDto(user);

        result.UserId.Should().Be(userId);
        result.Username.Should().Be("test-user");
        result.Email.Should().Be("user@example.com");
        result.Coins.Should().Be(123);
        result.Experience.Should().Be(456);
        result.AmountSolved.Should().Be(7);
        result.CohortId.Should().Be(cohortId);
        result.S3AvatarUrl.Should().Be(string.Empty);
    }

    [Fact]
    public void ToUserProfileDto_WhenUserNameAndEmailAreNull_UsesEmptyStrings()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = null,
            Email = null
        };

        var result = ProfileMapper.ToUserProfileDto(user);

        result.Username.Should().BeEmpty();
        result.Email.Should().BeEmpty();
    }
}