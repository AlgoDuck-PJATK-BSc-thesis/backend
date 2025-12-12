using AlgoDuck.Modules.Auth.Shared.Exceptions;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Exceptions;

public sealed class TokenExceptionTests
{
    [Fact]
    public void Ctor_SetsExpectedCodeAndMessage()
    {
        var ex = new TokenException("bad token");

        Assert.Equal("token_error", ex.Code);
        Assert.Equal("bad token", ex.Message);
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public void Ctor_WithInnerException_SetsExpectedCodeAndInnerException()
    {
        var inner = new Exception("inner");
        var ex = new TokenException("bad token", inner);

        Assert.Equal("token_error", ex.Code);
        Assert.Equal("bad token", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}