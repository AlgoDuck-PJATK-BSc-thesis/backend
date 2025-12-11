using FluentAssertions;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;

namespace AlgoDuck.Tests.Modules.Cohort.Shared.Exceptions;

public sealed class CohortExceptionTests
{
    private sealed class TestCohortException : CohortException
    {
        public TestCohortException(string code, string message)
            : base(code, message)
        {
        }

        public TestCohortException(string code, string message, Exception? innerException)
            : base(code, message, innerException)
        {
        }
    }

    [Fact]
    public void Ctor_WithCodeAndMessage_SetsProperties()
    {
        var code = "test_code";
        var message = "Something went wrong";

        var exception = new TestCohortException(code, message);

        exception.Code.Should().Be(code);
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void Ctor_WithInnerException_SetsInnerException()
    {
        var code = "test_code";
        var message = "Something went wrong";
        var inner = new InvalidOperationException("inner");

        var exception = new TestCohortException(code, message, inner);

        exception.Code.Should().Be(code);
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeSameAs(inner);
    }
}