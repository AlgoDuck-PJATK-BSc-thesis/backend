using AlgoDuck.Modules.Auth.Shared.Exceptions;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Exceptions;

public sealed class TwoFactorExceptionTests
{
    [Fact]
    public void Ctor_SetsExpectedCodeAndMessage()
    {
        var ex = new TwoFactorException("2fa");

        Assert.Equal("two_factor_error", ex.Code);
        Assert.Equal("2fa", ex.Message);
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public void Ctor_WithInnerException_SetsExpectedCodeAndInnerException()
    {
        var inner = new Exception("inner");
        var ex = new TwoFactorException("2fa", inner);

        Assert.Equal("two_factor_error", ex.Code);
        Assert.Equal("2fa", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}