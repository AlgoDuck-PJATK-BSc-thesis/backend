using AlgoDuck.Modules.Auth.Shared.Exceptions;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Exceptions;

public sealed class EmailVerificationExceptionTests
{
    [Fact]
    public void Ctor_SetsExpectedCodeAndMessage()
    {
        var ex = new EmailVerificationException("boom");

        Assert.Equal("email_verification_error", ex.Code);
        Assert.Equal("boom", ex.Message);
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public void Ctor_WithInnerException_SetsExpectedCodeAndInnerException()
    {
        var inner = new Exception("inner");
        var ex = new EmailVerificationException("boom", inner);

        Assert.Equal("email_verification_error", ex.Code);
        Assert.Equal("boom", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}