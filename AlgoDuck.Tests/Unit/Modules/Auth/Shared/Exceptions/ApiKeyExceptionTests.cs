using AlgoDuck.Modules.Auth.Shared.Exceptions;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Exceptions;

public sealed class ApiKeyExceptionTests
{
    [Fact]
    public void Ctor_SetsExpectedCodeAndMessage()
    {
        var ex = new ApiKeyException("boom");

        Assert.Equal("api_key_error", ex.Code);
        Assert.Equal("boom", ex.Message);
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public void Ctor_WithInnerException_SetsExpectedCodeAndInnerException()
    {
        var inner = new Exception("inner");
        var ex = new ApiKeyException("boom", inner);

        Assert.Equal("api_key_error", ex.Code);
        Assert.Equal("boom", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}