using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentAssertions;

namespace AlgoDuck.Tests.Modules.User.Shared.Exceptions;

public sealed class ProfileExceptionTests
{
    [Fact]
    public void Ctor_Parameterless_SetsDefaultValues()
    {
        var ex = new ProfileException();

        ex.Message.Should().NotBeNullOrWhiteSpace();
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Ctor_WithMessage_SetsMessage()
    {
        const string message = "custom message";

        var ex = new ProfileException(message);

        ex.Message.Should().Be(message);
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Ctor_WithMessageAndInner_SetsAllProperties()
    {
        const string message = "custom message";
        var inner = new InvalidOperationException("inner");

        var ex = new ProfileException(message, inner);

        ex.Message.Should().Be(message);
        ex.InnerException.Should().BeSameAs(inner);
    }
}