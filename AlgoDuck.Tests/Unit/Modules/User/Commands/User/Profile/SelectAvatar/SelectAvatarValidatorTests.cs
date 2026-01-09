using AlgoDuck.Modules.User.Commands.User.Profile.SelectAvatar;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Unit.Modules.User.Commands.User.Profile.SelectAvatar;

public sealed class SelectAvatarValidatorTests
{
    [Fact]
    public void Validate_WhenItemIdEmpty_ThenHasValidationError()
    {
        var validator = new SelectAvatarValidator();
        var dto = new SelectAvatarDto
        {
            ItemId = Guid.Empty
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ItemId);
    }

    [Fact]
    public void Validate_WhenValid_ThenHasNoValidationErrors()
    {
        var validator = new SelectAvatarValidator();
        var dto = new SelectAvatarDto
        {
            ItemId = Guid.NewGuid()
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}