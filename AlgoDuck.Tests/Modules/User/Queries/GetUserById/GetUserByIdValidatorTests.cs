using AlgoDuck.Modules.User.Queries.User.Profile.GetUserById;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.User.Queries.GetUserById;

public sealed class GetUserByIdValidatorTests
{
    [Fact]
    public void Validate_WhenUserIdEmpty_ThenHasValidationError()
    {
        var validator = new GetUserByIdValidator();

        var result = validator.TestValidate(new GetUserByIdRequestDto
        {
            UserId = Guid.Empty
        });

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WhenUserIdValid_ThenHasNoValidationErrors()
    {
        var validator = new GetUserByIdValidator();

        var result = validator.TestValidate(new GetUserByIdRequestDto
        {
            UserId = Guid.NewGuid()
        });

        result.ShouldNotHaveAnyValidationErrors();
    }
}