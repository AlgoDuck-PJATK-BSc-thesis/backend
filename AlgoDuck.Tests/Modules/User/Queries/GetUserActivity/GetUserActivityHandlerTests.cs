using AlgoDuck.Models;
using AlgoDuck.Modules.User.Queries.User.Activity.GetUserActivity;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Modules.User.Queries.GetUserActivity;

public sealed class GetUserActivityHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenRequestHasInvalidPage_ThenNormalizesTo1()
    {
        var userId = Guid.NewGuid();

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetUserSolutionsAsync(userId, 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<UserSolution>());

        var handler = new GetUserActivityHandler(userRepository.Object);

        await handler.HandleAsync(userId, new GetUserActivityRequestDto
        {
            Page = 0,
            PageSize = 20
        }, CancellationToken.None);

        userRepository.Verify(x => x.GetUserSolutionsAsync(userId, 0, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRequestHasInvalidPageSize_ThenNormalizesTo20()
    {
        var userId = Guid.NewGuid();

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetUserSolutionsAsync(userId, 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<UserSolution>());

        var handler = new GetUserActivityHandler(userRepository.Object);

        await handler.HandleAsync(userId, new GetUserActivityRequestDto
        {
            Page = 1,
            PageSize = 0
        }, CancellationToken.None);

        userRepository.Verify(x => x.GetUserSolutionsAsync(userId, 0, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenSolutionsReturned_ThenMapsToUserActivityDto()
    {
        var userId = Guid.NewGuid();

        var solutionId = Guid.NewGuid();
        var problemId = Guid.NewGuid();
        var statusId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var problem = new Models.Problem
        {
            ProblemId = problemId,
            ProblemTitle = "Two Sum",
            CreatedAt = DateTime.UtcNow,
            CategoryId = Guid.NewGuid(),
            DifficultyId = Guid.NewGuid(),
            Category = null!,
            Difficulty = null!
        };

        var solutions = new List<UserSolution>
        {
            new()
            {
                SolutionId = solutionId,
                UserId = userId,
                ProblemId = problemId,
                CodeRuntimeSubmitted = 123,
                CreatedAt = createdAt,
                Problem = problem,
                User = null!
            }
        }.AsReadOnly();

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(x => x.GetUserSolutionsAsync(userId, 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solutions);

        var handler = new GetUserActivityHandler(userRepository.Object);

        var result = await handler.HandleAsync(userId, new GetUserActivityRequestDto
        {
            Page = 1,
            PageSize = 20
        }, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(solutionId, result[0].SolutionId);
        Assert.Equal(problemId, result[0].ProblemId);
        Assert.Equal("Two Sum", result[0].ProblemName);
        Assert.Equal(123, result[0].CodeRuntimeSubmitted);
        Assert.Equal(createdAt, result[0].SubmittedAt);
    }
}