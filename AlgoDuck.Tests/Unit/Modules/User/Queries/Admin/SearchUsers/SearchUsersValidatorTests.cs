using AlgoDuck.Modules.User.Queries.Admin.SearchUsers;

namespace AlgoDuck.Tests.Unit.Modules.User.Queries.Admin.SearchUsers;

public sealed class SearchUsersValidatorTests
{
    [Fact]
    public void Validate_WhenValid_IsValid()
    {
        var v = new SearchUsersValidator();

        var dto = new SearchUsersDto
        {
            Query = "john",
            UsernamePage = 1,
            UsernamePageSize = 20,
            EmailPage = 1,
            EmailPageSize = 20
        };

        var result = v.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenQueryEmpty_IsInvalid()
    {
        var v = new SearchUsersValidator();

        var dto = new SearchUsersDto
        {
            Query = "",
            UsernamePage = 1,
            UsernamePageSize = 20,
            EmailPage = 1,
            EmailPageSize = 20
        };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Query");
    }

    [Fact]
    public void Validate_WhenPagesOrSizesInvalid_IsInvalid()
    {
        var v = new SearchUsersValidator();

        var dto = new SearchUsersDto
        {
            Query = "john",
            UsernamePage = 0,
            UsernamePageSize = 0,
            EmailPage = 0,
            EmailPageSize = 201
        };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
    }
}