using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Validators;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Validators;

public sealed class ApiKeyValidatorTests
{
    [Fact]
    public void ValidateName_WhenNull_ThenThrowsValidationException()
    {
        var validator = new ApiKeyValidator();

        Assert.Throws<ValidationException>(() => validator.ValidateName(null!));
    }

    [Fact]
    public void ValidateName_WhenEmpty_ThenThrowsValidationException()
    {
        var validator = new ApiKeyValidator();

        var ex = Assert.Throws<ValidationException>(() => validator.ValidateName(""));

        Assert.Contains("API key name", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateName_WhenWhitespace_ThenThrowsValidationException()
    {
        var validator = new ApiKeyValidator();

        Assert.Throws<ValidationException>(() => validator.ValidateName("   "));
    }

    [Fact]
    public void ValidateName_WhenTooLong_ThenThrowsValidationException()
    {
        var validator = new ApiKeyValidator();
        var name = new string('a', 257);

        var ex = Assert.Throws<ValidationException>(() => validator.ValidateName(name));

        Assert.Contains("API key name", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("at most", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("256", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateName_WhenMaxLength_ThenDoesNotThrow()
    {
        var validator = new ApiKeyValidator();
        var name = new string('a', 256);

        var ex = Record.Exception(() => validator.ValidateName(name));

        Assert.Null(ex);
    }

    [Fact]
    public void ValidateName_WhenValidShortName_ThenDoesNotThrow()
    {
        var validator = new ApiKeyValidator();

        var ex = Record.Exception(() => validator.ValidateName("my-key"));

        Assert.Null(ex);
    }
}
