using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using FluentAssertions;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Shared.Exceptions;

public sealed class ChatValidationExceptionTests
{
    [Fact]
    public void Ctor_WithMessage_SetsCodeAndMessage_LeavesCategoryNull()
    {
        var message = "bad chat message";

        var ex = new ChatValidationException(message);

        ex.Code.Should().Be("chat_validation_error");
        ex.Message.Should().Be(message);
        ex.Category.Should().BeNull();
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Ctor_WithMessageAndCategory_SetsCategory()
    {
        var message = "bad chat message";
        var category = "toxicity";

        var ex = new ChatValidationException(message, category);

        ex.Code.Should().Be("chat_validation_error");
        ex.Message.Should().Be(message);
        ex.Category.Should().Be(category);
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Ctor_WithMessageStatusCodeAndInner_SetsInnerException_CategoryNull()
    {
        var message = "bad chat message";
        var inner = new InvalidOperationException("inner");
        var statusCode = 400;

        var ex = new ChatValidationException(message, statusCode, inner);

        ex.Code.Should().Be("chat_validation_error");
        ex.Message.Should().Be(message);
        ex.Category.Should().BeNull();
        ex.InnerException.Should().BeSameAs(inner);
    }
}
