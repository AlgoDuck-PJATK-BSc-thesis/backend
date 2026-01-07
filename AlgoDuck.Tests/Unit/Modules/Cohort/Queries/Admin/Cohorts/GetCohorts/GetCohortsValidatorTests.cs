using AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.GetCohorts;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Queries.Admin.Cohorts.GetCohorts;

public sealed class GetCohortsValidatorTests
{
    [Fact]
    public void Validate_WhenValid_IsValid()
    {
        var v = new AdminGetCohortsValidator();

        var result = v.Validate(new AdminGetCohortsDto
        {
            Page = 1,
            PageSize = 20
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenPageLessThan1_IsInvalid()
    {
        var v = new AdminGetCohortsValidator();

        var result = v.Validate(new AdminGetCohortsDto
        {
            Page = 0,
            PageSize = 20
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Page");
    }

    [Fact]
    public void Validate_WhenPageSizeOutOfRange_IsInvalid()
    {
        var v = new AdminGetCohortsValidator();

        var r1 = v.Validate(new AdminGetCohortsDto { Page = 1, PageSize = 0 });
        Assert.False(r1.IsValid);

        var r2 = v.Validate(new AdminGetCohortsDto { Page = 1, PageSize = 201 });
        Assert.False(r2.IsValid);
    }
}