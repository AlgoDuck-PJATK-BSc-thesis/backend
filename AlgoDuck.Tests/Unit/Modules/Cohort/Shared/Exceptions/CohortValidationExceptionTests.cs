using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using FluentAssertions;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Shared.Exceptions;

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
    public void Ctor_WithMessageStatusCodeAndInner_SetsAllProperties()
    {
        var message = "validation failed";
        var inner = new ArgumentException("inner");
        var statusCode = 400;

        var ex = new CohortValidationException(message, statusCode, inner);

        ex.Code.Should().Be("cohort_validation_error");
        ex.Message.Should().Be(message);
        ex.InnerException.Should().BeSameAs(inner);
    }
}