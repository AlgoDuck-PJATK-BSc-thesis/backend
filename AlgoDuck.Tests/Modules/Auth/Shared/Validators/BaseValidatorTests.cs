using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Validators;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Validators;

public sealed class BaseValidatorTests
{
    private sealed class TestValidator : BaseValidator
    {
        public void CallEnsure(bool condition, string message) => Ensure(condition, message);
        public void CallEnsureNotNullOrWhiteSpace(string value, string field) => EnsureNotNullOrWhiteSpace(value, field);
        public void CallEnsureMaxLength(string value, int maxLen, string field) => EnsureMaxLength(value, maxLen, field);
    }

    [Fact]
    public void Ensure_WhenConditionFalse_ThenThrowsValidationException()
    {
        var v = new TestValidator();

        Assert.Throws<ValidationException>(() => v.CallEnsure(false, "boom"));
    }

    [Fact]
    public void Ensure_WhenConditionTrue_ThenDoesNotThrow()
    {
        var v = new TestValidator();

        var ex = Record.Exception(() => v.CallEnsure(true, "boom"));

        Assert.Null(ex);
    }

    [Fact]
    public void EnsureNotNullOrWhiteSpace_WhenNull_ThenThrowsValidationException()
    {
        var v = new TestValidator();

        Assert.Throws<ValidationException>(() => v.CallEnsureNotNullOrWhiteSpace(null!, "Field"));
    }

    [Fact]
    public void EnsureNotNullOrWhiteSpace_WhenWhitespace_ThenThrowsValidationException()
    {
        var v = new TestValidator();

        Assert.Throws<ValidationException>(() => v.CallEnsureNotNullOrWhiteSpace("   ", "Field"));
    }

    [Fact]
    public void EnsureNotNullOrWhiteSpace_WhenValid_ThenDoesNotThrow()
    {
        var v = new TestValidator();

        var ex = Record.Exception(() => v.CallEnsureNotNullOrWhiteSpace("x", "Field"));

        Assert.Null(ex);
    }

    [Fact]
    public void EnsureMaxLength_WhenTooLong_ThenThrowsValidationException()
    {
        var v = new TestValidator();

        Assert.Throws<ValidationException>(() => v.CallEnsureMaxLength("abcd", 3, "Field"));
    }

    [Fact]
    public void EnsureMaxLength_WhenEqualOrShorter_ThenDoesNotThrow()
    {
        var v = new TestValidator();

        var ex1 = Record.Exception(() => v.CallEnsureMaxLength("abc", 3, "Field"));
        var ex2 = Record.Exception(() => v.CallEnsureMaxLength("ab", 3, "Field"));

        Assert.Null(ex1);
        Assert.Null(ex2);
    }
}
