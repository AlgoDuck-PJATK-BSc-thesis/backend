using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Validators;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Validators;

public sealed class TokenValidatorTests
{
    [Fact]
    public void EnsureNotExpired_WhenExpired_ThenThrowsValidationException()
    {
        var validator = new TokenValidator();
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(-1);

        Assert.Throws<ValidationException>(() => validator.EnsureNotExpired(expiresAt));
    }

    [Fact]
    public void EnsureNotExpired_WhenInFuture_ThenDoesNotThrow()
    {
        var validator = new TokenValidator();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(1);

        var ex = Record.Exception(() => validator.EnsureNotExpired(expiresAt));

        Assert.Null(ex);
    }

    [Fact]
    public void EnsureNotRevoked_WhenRevoked_ThenThrowsValidationException()
    {
        var validator = new TokenValidator();

        Assert.Throws<ValidationException>(() => validator.EnsureNotRevoked(true));
    }

    [Fact]
    public void EnsureNotRevoked_WhenNotRevoked_ThenDoesNotThrow()
    {
        var validator = new TokenValidator();

        var ex = Record.Exception(() => validator.EnsureNotRevoked(false));

        Assert.Null(ex);
    }
}