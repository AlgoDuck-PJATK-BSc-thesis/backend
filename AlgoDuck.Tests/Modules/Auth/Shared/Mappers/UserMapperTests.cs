using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Mappers;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Mappers;

public sealed class UserMapperTests
{
    [Fact]
    public void ToAuthUserDto_MapsAllFields_AndUsesEmptyStringsForNulls()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = null,
            Email = null,
            EmailConfirmed = true
        };

        var dto = UserMapper.ToAuthUserDto(user);

        Assert.Equal(user.Id, dto.Id);
        Assert.Equal(string.Empty, dto.UserName);
        Assert.Equal(string.Empty, dto.Email);
        Assert.True(dto.EmailConfirmed);
    }

    [Fact]
    public void ToAuthUserDto_MapsNonNullStrings()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            Email = "alice@example.com",
            EmailConfirmed = false
        };

        var dto = UserMapper.ToAuthUserDto(user);

        Assert.Equal("alice", dto.UserName);
        Assert.Equal("alice@example.com", dto.Email);
        Assert.False(dto.EmailConfirmed);
    }
}