using AlgoDuck.Models;
using AlgoDuck.Modules.User.Queries.SearchUsers;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Modules.User.Queries.SearchUsers;

public sealed class SearchUsersHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsUsers_ThenMapsToResultDtos()
    {
        var u1 = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            Email = "alice@test.local",
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var u2 = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "bob",
            Email = "bob@test.local",
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.SearchAsync("a", 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApplicationUser> { u1, u2 }.AsReadOnly());

        var handler = new SearchUsersHandler(userRepository.Object);

        var result = await handler.HandleAsync(new SearchUsersDto
        {
            Query = "a",
            Page = 2,
            PageSize = 5
        }, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(u1.Id, result[0].UserId);
        Assert.Equal("alice", result[0].Username);
        Assert.Equal("alice@test.local", result[0].Email);

        Assert.Equal(u2.Id, result[1].UserId);
        Assert.Equal("bob", result[1].Username);
        Assert.Equal("bob@test.local", result[1].Email);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNameOrEmailNull_ThenMapsToEmptyStrings()
    {
        var u = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = null,
            Email = null,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.SearchAsync("", 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ApplicationUser> { u }.AsReadOnly());

        var handler = new SearchUsersHandler(userRepository.Object);

        var result = await handler.HandleAsync(new SearchUsersDto
        {
            Query = "",
            Page = 1,
            PageSize = 20
        }, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(u.Id, result[0].UserId);
        Assert.Equal(string.Empty, result[0].Username);
        Assert.Equal(string.Empty, result[0].Email);
    }
}