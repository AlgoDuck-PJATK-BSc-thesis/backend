using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using FluentAssertions;

namespace AlgoDuck.Tests.Modules.Cohort.Shared.Exceptions;

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
    public void Ctor_WithMessageCategoryAndInner_SetsAllProperties()
    {
        var message = "bad chat message";
        var category = "self_harm";
        var inner = new InvalidOperationException("inner");

        var ex = new ChatValidationException(message, category, inner);

        ex.Code.Should().Be("chat_validation_error");
        ex.Message.Should().Be(message);
        ex.Category.Should().Be(category);
        ex.InnerException.Should().BeSameAs(inner);
    }
}