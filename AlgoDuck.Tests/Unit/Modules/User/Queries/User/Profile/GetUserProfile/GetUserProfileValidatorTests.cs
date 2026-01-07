using AlgoDuck.Modules.User.Queries.User.Profile.GetUserProfile;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.User.Profile.GetUserProfile;

public sealed class GetUserProfileValidatorTests
{
    [Fact]
    public void Validate_WhenUserIdEmpty_ThenHasValidationError()
    {
        var validator = new GetUserProfileValidator();

        var result = validator.TestValidate(Guid.Empty);

        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_WhenUserIdValid_ThenHasNoValidationErrors()
    {
        var validator = new GetUserProfileValidator();

        var result = validator.TestValidate(Guid.NewGuid());

        result.ShouldNotHaveAnyValidationErrors();
    }
}