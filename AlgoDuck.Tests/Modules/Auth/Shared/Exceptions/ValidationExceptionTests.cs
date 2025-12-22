using AlgoDuck.Modules.Auth.Shared.Exceptions;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Exceptions;

public sealed class ValidationExceptionTests
{
    [Fact]
    public void Ctor_SetsExpectedCodeAndMessage()
    {
        var ex = new ValidationException("invalid");

        Assert.Equal("auth_validation_error", ex.Code);
        Assert.Equal("invalid", ex.Message);
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public void Ctor_WithInnerException_SetsExpectedCodeAndInnerException()
    {
        var inner = new Exception("inner");
        var ex = new ValidationException("invalid", inner);

        Assert.Equal("auth_validation_error", ex.Code);
        Assert.Equal("invalid", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}