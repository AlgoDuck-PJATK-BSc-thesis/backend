using AlgoDuck.Modules.User.Queries.GetUserConfig;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.User.Queries.GetUserConfig;

public sealed class GetUserConfigValidatorTests
{
    [Fact]
    public void Validate_WhenUserIdEmpty_ThenHasValidationError()
    {
        var validator = new GetUserConfigValidator();

        var result = validator.TestValidate(new GetUserConfigRequestDto
        {
            UserId = Guid.Empty
        });

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WhenUserIdValid_ThenHasNoValidationErrors()
    {
        var validator = new GetUserConfigValidator();

        var result = validator.TestValidate(new GetUserConfigRequestDto
        {
            UserId = Guid.NewGuid()
        });

        result.ShouldNotHaveAnyValidationErrors();
    }
}