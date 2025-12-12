using AlgoDuck.Modules.Auth.Shared.Exceptions;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Exceptions;

public sealed class PermissionExceptionTests
{
    [Fact]
    public void Ctor_SetsExpectedCodeAndMessage()
    {
        var ex = new PermissionException("nope");

        Assert.Equal("permission_denied", ex.Code);
        Assert.Equal("nope", ex.Message);
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public void Ctor_WithInnerException_SetsExpectedCodeAndInnerException()
    {
        var inner = new Exception("inner");
        var ex = new PermissionException("nope", inner);

        Assert.Equal("permission_denied", ex.Code);
        Assert.Equal("nope", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}