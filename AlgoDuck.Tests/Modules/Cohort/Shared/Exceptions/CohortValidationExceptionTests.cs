using FluentAssertions;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;

namespace AlgoDuck.Tests.Modules.Cohort.Shared.Exceptions;

public sealed class CohortValidationExceptionTests
{
    [Fact]
    public void Ctor_WithMessage_SetsMessageAndCode()
    {
        var message = "validation failed";

        var ex = new CohortValidationException(message);

        ex.Code.Should().Be("cohort_validation_error");
        ex.Message.Should().Be(message);
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Ctor_WithMessageAndInner_SetsAllProperties()
    {
        var message = "validation failed";
        var inner = new ArgumentException("inner");

        var ex = new CohortValidationException(message, inner);

        ex.Code.Should().Be("cohort_validation_error");
        ex.Message.Should().Be(message);
        ex.InnerException.Should().BeSameAs(inner);
    }
}