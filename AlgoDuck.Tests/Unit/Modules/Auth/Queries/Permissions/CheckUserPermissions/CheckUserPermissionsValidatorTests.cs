using AlgoDuck.Modules.Auth.Queries.Permissions.CheckUserPermissions;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Queries.Permissions.CheckUserPermissions;

public sealed class CheckUserPermissionsValidatorTests
{
    [Fact]
    public void Validate_WhenPermissionsNull_Throws()
    {
        var validator = new CheckUserPermissionsValidator();
        var dto = new PermissionsDto { Permissions = null! };

        var ex = Record.Exception(() => validator.Validate(dto));

        Assert.NotNull(ex);
        Assert.IsType<NullReferenceException>(ex);
    }

    [Fact]
    public void Validate_WhenPermissionsEmpty_Fails()
    {
        var validator = new CheckUserPermissionsValidator();
        var dto = new PermissionsDto { Permissions = Array.Empty<string>() };

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "At least one permission must be provided.");
    }

    [Fact]
    public void Validate_WhenPermissionsHasItems_Passes()
    {
        var validator = new CheckUserPermissionsValidator();
        var dto = new PermissionsDto { Permissions = new[] { "" } };

        var result = validator.Validate(dto);

        Assert.True(result.IsValid);
    }
}