using AlgoDuck.Modules.Auth.Shared.Exceptions;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Exceptions;

public sealed class AuthExceptionTests
{
    private sealed class TestAuthException : AuthException
    {
        public TestAuthException(string code, string message) : base(code, message) { }
        public TestAuthException(string code, string message, Exception? inner) : base(code, message, inner) { }
    }

    [Fact]
    public void Ctor_SetsCodeAndMessage()
    {
        var ex = new TestAuthException("code_x", "msg");

        Assert.Equal("code_x", ex.Code);
        Assert.Equal("msg", ex.Message);
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public void Ctor_WithInnerException_SetsAllFields()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new TestAuthException("code_y", "msg2", inner);

        Assert.Equal("code_y", ex.Code);
        Assert.Equal("msg2", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}