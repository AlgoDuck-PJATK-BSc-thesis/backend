using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentAssertions;

namespace AlgoDuck.Tests.Unit.Modules.User.Shared.Exceptions;

public sealed class UserNotFoundExceptionTests
{
    [Fact]
    public void Ctor_Parameterless_SetsDefaultValues()
    {
        var ex = new UserNotFoundException();

        ex.UserId.Should().Be(Guid.Empty);
        ex.Message.Should().NotBeNullOrWhiteSpace();
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Ctor_WithUserId_SetsMessageAndUserId()
    {
        var userId = Guid.NewGuid();

        var ex = new UserNotFoundException(userId);

        ex.UserId.Should().Be(userId);
        ex.Message.Should().Contain(userId.ToString());
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Ctor_WithMessage_SetsMessage()
    {
        const string message = "custom message";

        var ex = new UserNotFoundException(message);

        ex.UserId.Should().Be(Guid.Empty);
        ex.Message.Should().Be(message);
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Ctor_WithMessageAndInner_SetsAllProperties()
    {
        const string message = "custom message";
        var inner = new InvalidOperationException("inner");

        var ex = new UserNotFoundException(message, inner);

        ex.UserId.Should().Be(Guid.Empty);
        ex.Message.Should().Be(message);
        ex.InnerException.Should().BeSameAs(inner);
    }
}