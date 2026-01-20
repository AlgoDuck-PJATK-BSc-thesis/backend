using AlgoDuck.Modules.User.Queries.Admin.GetUsers;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.Admin.GetUsers;

public sealed class GetUsersValidatorTests
{
    [Fact]
    public void Validate_WhenValid_IsValid()
    {
        var v = new GetUsersValidator();
        var result = v.Validate(new GetUsersDto { Page = 1, PageSize = 20 });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenPageLessThan1_IsInvalid()
    {
        var v = new GetUsersValidator();
        var result = v.Validate(new GetUsersDto { Page = 0, PageSize = 20 });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenPageSizeOutOfRange_IsInvalid()
    {
        var v = new GetUsersValidator();

        var r1 = v.Validate(new GetUsersDto { Page = 1, PageSize = 0 });
        Assert.False(r1.IsValid);

        var r2 = v.Validate(new GetUsersDto { Page = 1, PageSize = 201 });
        Assert.False(r2.IsValid);
    }
}