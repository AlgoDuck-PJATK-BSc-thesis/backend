using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using FluentAssertions;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Shared.Exceptions;

public sealed class CohortNotFoundExceptionTests
{
    [Fact]
    public void Ctor_WithCohortId_SetsMessageAndCode()
    {
        var cohortId = Guid.NewGuid();

        var ex = new CohortNotFoundException(cohortId);

        ex.Code.Should().Be("cohort_not_found");
        ex.Message.Should().Contain(cohortId.ToString());
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Ctor_WithMessage_SetsMessageAndCode()
    {
        var message = "custom message";

        var ex = new CohortNotFoundException(message);

        ex.Code.Should().Be("cohort_not_found");
        ex.Message.Should().Be(message);
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Ctor_WithMessageAndInner_SetsAllProperties()
    {
        var message = "custom message";
        var inner = new InvalidOperationException("inner");

        var ex = new CohortNotFoundException(message, inner);

        ex.Code.Should().Be("cohort_not_found");
        ex.Message.Should().Be(message);
        ex.InnerException.Should().BeSameAs(inner);
    }
}