using AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.SearchCohorts;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Queries.Admin.Cohorts.SearchCohorts;

public sealed class SearchCohortsValidatorTests
{
    [Fact]
    public void Validate_WhenValid_IsValid()
    {
        var v = new SearchCohortsValidator();

        var result = v.Validate(new SearchCohortsDto
        {
            Query = "abc",
            Page = 1,
            PageSize = 20
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenQueryEmpty_IsInvalid()
    {
        var v = new SearchCohortsValidator();

        var result = v.Validate(new SearchCohortsDto
        {
            Query = "",
            Page = 1,
            PageSize = 20
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Query");
    }

    [Fact]
    public void Validate_WhenPageOrPageSizeInvalid_IsInvalid()
    {
        var v = new SearchCohortsValidator();

        var r1 = v.Validate(new SearchCohortsDto { Query = "x", Page = 0, PageSize = 20 });
        Assert.False(r1.IsValid);

        var r2 = v.Validate(new SearchCohortsDto { Query = "x", Page = 1, PageSize = 0 });
        Assert.False(r2.IsValid);

        var r3 = v.Validate(new SearchCohortsDto { Query = "x", Page = 1, PageSize = 201 });
        Assert.False(r3.IsValid);
    }
}