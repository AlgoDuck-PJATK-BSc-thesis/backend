using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentAssertions;

namespace AlgoDuck.Tests.Unit.Modules.User.Shared.Exceptions;

public sealed class ValidationExceptionTests
{
    [Fact]
    public void Ctor_WithMessage_InitializesEmptyErrors()
    {
        const string message = "validation failed";

        var ex = new ValidationException(message);

        ex.Message.Should().Be(message);
        ex.Errors.Should().NotBeNull();
        ex.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_WithMessageAndErrors_CopiesErrors()
    {
        const string message = "validation failed";
        var errors = new Dictionary<string, string[]>
        {
            ["Email"] = new[] { "Invalid email" },
            ["Password"] = new[] { "Too short", "Must contain a number" }
        };

        var ex = new ValidationException(message, errors);

        ex.Message.Should().Be(message);
        ex.Errors.Should().NotBeNull();
        ex.Errors.Should().BeEquivalentTo(errors);
        ((object)ex.Errors).Should().NotBeSameAs(errors);
    }
}